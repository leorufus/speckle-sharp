﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements.Revit;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using Column = Objects.BuiltElements.Column;
using DB = Autodesk.Revit.DB;
using Line = Objects.Geometry.Line;
using Point = Objects.Geometry.Point;

namespace Objects.Converter.Revit
{
  public partial class ConverterRevit
  {
    public List<ApplicationPlaceholderObject> ColumnToNative(Column speckleColumn)
    {
      if (speckleColumn.baseLine == null)
      {
        throw new Exception("Only line based Beams are currently supported.");
      }

      DB.FamilySymbol familySymbol = GetElementType<FamilySymbol>(speckleColumn); ;
      var baseLine = CurveToNative(speckleColumn.baseLine).get_Item(0);
      
      // If the start point elevation is higher than the end point elevation, reverse the line.
      if (baseLine.GetEndPoint(0).Z > baseLine.GetEndPoint(1).Z)
      {
        baseLine = DB.Line.CreateBound(baseLine.GetEndPoint(1), baseLine.GetEndPoint(0));
      }

      DB.Level level = null;
      DB.Level topLevel = null;
      DB.FamilyInstance revitColumn = null;
      var structuralType = StructuralType.Column;
      var isLineBased = true;

      var speckleRevitColumn = speckleColumn as RevitColumn;

      if (speckleRevitColumn != null)
      {
        level = LevelToNative(speckleRevitColumn.level);
        topLevel = LevelToNative(speckleRevitColumn.topLevel);
        structuralType = speckleRevitColumn.structural ? StructuralType.Column : StructuralType.NonStructural;
        //non slanted columns are point based
        isLineBased = speckleRevitColumn.isSlanted;
      }

      if (level == null)
      {
        level = LevelToNative(LevelFromCurve(baseLine));
        topLevel = LevelToNative(LevelFromPoint(baseLine.GetEndPoint(1)));
      }

      //try update existing 
      var docObj = GetExistingElementByApplicationId(speckleColumn.applicationId);
      if (docObj != null)
      {
        try
        {
          var revitType = Doc.GetElement(docObj.GetTypeId()) as ElementType;

          // if family changed, tough luck. delete and let us create a new one.
          if (familySymbol.FamilyName != revitType.FamilyName)
          {
            Doc.Delete(docObj.Id);
          }
          else
          {
            revitColumn = (DB.FamilyInstance)docObj;
            switch(revitColumn.Location)
            {
              case LocationCurve crv:
                crv.Curve = baseLine;
                break;
              case LocationPoint pt:
                pt.Point = baseLine.GetEndPoint(0);
                break;
            }

            // check for a type change
            if (!string.IsNullOrEmpty(familySymbol.FamilyName) && familySymbol.FamilyName != revitType.Name)
            {
              revitColumn.ChangeTypeId(familySymbol.Id);
            }
          }
        }
        catch { }
      }

      if (revitColumn == null && isLineBased)
      {
        revitColumn = Doc.Create.NewFamilyInstance(baseLine, familySymbol, level, structuralType);
      }

      //try with a point based column
      if (speckleRevitColumn != null && revitColumn == null && !isLineBased)
      {
        var start = baseLine.GetEndPoint(0);
        var end = baseLine.GetEndPoint(1);

        var basePoint = start.Z < end.Z ? start : end; // pick the lowest
        revitColumn = Doc.Create.NewFamilyInstance(basePoint, familySymbol, level, StructuralType.NonStructural);

        //rotate, we know it must be a RevitColumn
        var axis = DB.Line.CreateBound(new XYZ(basePoint.X, basePoint.Y, 0), new XYZ(basePoint.X, basePoint.Y, 1000));
        (revitColumn.Location as LocationPoint).Rotate(axis, speckleRevitColumn.rotation - (revitColumn.Location as LocationPoint).Rotation);
      }

      if(revitColumn == null)
      {
        ConversionErrors.Add(new Error{ message = $"Failed to create column for {speckleColumn.applicationId}." });
        return null;
      }

      TrySetParam(revitColumn, BuiltInParameter.FAMILY_BASE_LEVEL_PARAM, level);
      TrySetParam(revitColumn, BuiltInParameter.FAMILY_TOP_LEVEL_PARAM, topLevel);

      if (speckleRevitColumn != null)
      {
        if (speckleRevitColumn.handFlipped != revitColumn.HandFlipped)
        {
          revitColumn.flipHand();
        }

        if (speckleRevitColumn.facingFlipped != revitColumn.FacingFlipped)
        {
          revitColumn.flipFacing();
        }

        SetOffsets(revitColumn, speckleRevitColumn);
        var exclusions = new List<string> { "Base Offset", "Top Offset" };
        SetInstanceParameters(revitColumn, speckleRevitColumn, exclusions);
      }

      var placeholders = new List<ApplicationPlaceholderObject>() { new ApplicationPlaceholderObject { applicationId = speckleColumn.applicationId, ApplicationGeneratedId = revitColumn.UniqueId, NativeObject = revitColumn } };

      // TODO: nested elements.

      return placeholders;
    }

    /// <summary>
    /// Some families eg columns, need offsets to be set in a specific way. This tries to cover that.
    /// </summary>
    /// <param name="speckleElement"></param>
    /// <param name="familyInstance"></param>
    private void SetOffsets(DB.FamilyInstance familyInstance, RevitColumn speckleRevitColumn)
    {
      var topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
      var baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
      var baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
      var topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);

      if (topLevelParam == null || baseLevelParam == null || baseOffsetParam == null || topOffsetParam == null)
      {
        return;
      }

      var baseOffset = UnitUtils.ConvertToInternalUnits(speckleRevitColumn.baseOffset, baseOffsetParam.DisplayUnitType);
      var topOffset = UnitUtils.ConvertToInternalUnits(speckleRevitColumn.topOffset, baseOffsetParam.DisplayUnitType);

      //these have been set previously
      DB.Level level = Doc.GetElement(baseLevelParam.AsElementId()) as DB.Level;
      DB.Level topLevel = Doc.GetElement(topLevelParam.AsElementId()) as DB.Level;

      //checking if BASE offset needs to be set before or after TOP offset
      if (topLevel != null && topLevel.Elevation + baseOffset <= level.Elevation)
      {
        baseOffsetParam.Set(baseOffset);
        topOffsetParam.Set(topOffset);
      }
      else
      {
        topOffsetParam.Set(topOffset);
        baseOffsetParam.Set(baseOffset);
      }

    }

    public RevitColumn ColumnToSpeckle(DB.FamilyInstance revitColumn)
    {
      //REVIT PARAMS > SPECKLE PROPS
      var baseLevelParam = revitColumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
      var topLevelParam = revitColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
      var baseOffsetParam = revitColumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
      var topOffsetParam = revitColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

      var speckleColumn = new RevitColumn();
      speckleColumn.type = Doc.GetElement(revitColumn.GetTypeId()).Name;
      speckleColumn.level = ConvertAndCacheLevel(baseLevelParam);
      speckleColumn.topLevel = ConvertAndCacheLevel(topLevelParam);
      speckleColumn.baseOffset = (double)ParameterToSpeckle(baseOffsetParam);
      speckleColumn.topOffset = (double)ParameterToSpeckle(topOffsetParam);
      speckleColumn.facingFlipped = revitColumn.FacingFlipped;
      speckleColumn.handFlipped = revitColumn.HandFlipped;
      speckleColumn.isSlanted = revitColumn.IsSlantedColumn;
      speckleColumn.structural = revitColumn.StructuralType == StructuralType.Column;

      //geometry
      var baseGeometry = LocationToSpeckle(revitColumn);
      var baseLine = baseGeometry as ICurve;

      //make line from point and height
      if (baseLine == null && baseGeometry is Point basePoint)
      {
        var elevation = (double)((RevitLevel)ParameterToSpeckle(topLevelParam)).elevation;
        baseLine = new Line(basePoint, new Point(basePoint.x, basePoint.y, elevation + speckleColumn.topOffset, ModelUnits), ModelUnits);
      }

      if (baseLine == null)
      {
        throw new Exception("Only line based Columns are currently supported.");
      }

      speckleColumn.baseLine = baseLine; //all speckle columns should be line based

      AddCommonRevitProps(speckleColumn, revitColumn);

      if (revitColumn.Location is LocationPoint)
      {
        speckleColumn.rotation = ((LocationPoint)revitColumn.Location).Rotation;
      }

      speckleColumn["@displayMesh"] = GetElementMesh(revitColumn);

      return speckleColumn;
    }


  }
}