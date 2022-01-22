using System.Collections.Generic;
using IA.Model.Structure;
using Objects.BuiltElements.IApp;
using Objects.Geometry;
using Speckle.Core.Models;
using Utilities.Geometry;

namespace Objects.Converter.IApp
{
    public partial class ConverterIAppGeometric
    {
        public List<ApplicationPlaceholderObject> BeamToNative(IAppBeam speckleBeam)
        {
            if (speckleBeam.baseLine == null)
            {
                throw new Speckle.Core.Logging.SpeckleException("Only line based Beams are currently supported.");
            }
            var speckleLine = speckleBeam.baseLine as Line;            
            
            Bar IAppbar;

            //try update existing 
            var isUpdate = false;

            var entity = GetExistingElementByApplicationId(speckleBeam.applicationId);
            if (entity != null)
            {
                IAppbar = entity as Bar;
                isUpdate = true;
            }
            else
            {
                IAppbar = new Bar(BarType.Beam) { Storey = speckleBeam.Storey };
            }
           

            IAppbar.GeometricElement = new GeometricBar()
            {
                StartPoint = new Punto3D() { X = speckleLine.start.x, Y = speckleLine.start.y, Z = speckleLine.start.z },
                EndPoint = new Punto3D() { X = speckleLine.end.x, Y = speckleLine.end.y, Z = speckleLine.end.z },
                Height = speckleBeam.Height,
                Width = speckleBeam.Width,
                StartPointOffset = new Offset() { Dx = 0, Dy = 0, Dz = 0 },
                EndPointOffset = new Offset() { Dx = 0, Dy = 0, Dz = 0 }
                
            };

            var placeholders = new List<ApplicationPlaceholderObject>() { new ApplicationPlaceholderObject { applicationId = speckleBeam.applicationId, ApplicationGeneratedId = IAppbar.GUID, NativeObject = IAppbar } };

            Report.Log($"{(isUpdate ? "Updated" : "Created")} AdaptiveComponent {IAppbar.GUID}");

            return placeholders;
        }

        private Base BeamToSpeckle(Bar IappBeam)
        {
            var bar = IappBeam.GeometricElement as GeometricBar;

            var baseLine = new Line(PointToSpeckle(bar.StartPoint), PointToSpeckle(bar.EndPoint));
            if (baseLine == null)
            {
                Report.Log($"Beam has no valid baseline, converting as generic element {IappBeam.GUID}");
                return null;
            }

            var speckleBeam = new IAppBeam();
            
            speckleBeam.baseLine = baseLine;
            speckleBeam.Storey = IappBeam.Storey;
            speckleBeam.displayMesh = SolidToSpeckle(IappBeam.GeometricElement.Solid);           

            Report.Log($"Converted Beam {IappBeam.GUID}");
            return speckleBeam;
        }
    }
}
