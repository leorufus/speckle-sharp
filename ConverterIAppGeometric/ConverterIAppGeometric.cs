using System;
using System.Collections.Generic;
using System.Linq;
using IA.Model;
using IA.Model.Structure;
using Objects.BuiltElements.IApp;
using Speckle.Core.Kits;
using Speckle.Core.Models;

using BE = Objects.BuiltElements;

namespace Objects.Converter.IApp
{
    public partial class ConverterIAppGeometric : ISpeckleConverter
    {
        #region ISpeckleConverter props
        public string Description => "Default Speckle Kit for IAppGeometric";

        public string Name => nameof(ConverterIAppGeometric);

        public string Author => "Ingenium";

        public string WebsiteOrEmail => "https://www.ingenium.com.uy/en/";

        public IEnumerable<string> GetServicedApplications() => new string[] { "IAppGeometric" };
        #endregion

        private static Project Project { get; set; }

        /// <summary>
        /// <para>To know which other objects are being converted, in order to sort relationships between them.
        /// For example, elements that have children use this to determine whether they should send their children out or not.</para>
        /// </summary>
        public List<ApplicationPlaceholderObject> ContextObjects { get; set; } = new List<ApplicationPlaceholderObject>();

        /// <summary>
        /// <para>To keep track of previously received objects from a given stream in here. If possible, conversions routines
        /// will edit an existing object, otherwise they will delete the old one and create the new one.</para>
        /// </summary>
        public List<ApplicationPlaceholderObject> PreviousContextObjects { get; set; } = new List<ApplicationPlaceholderObject>();

        public ConverterIAppGeometric(Project project) => Project = project;

        public ProgressReport Report { get; private set; } = new ProgressReport();

        public bool CanConvertToNative(Base @object)
        {
            return @object
            switch
            {
                BE.Beam _ => true,
                _ => false
            };            
        }

        public bool CanConvertToSpeckle(object @object)
        {
            return @object
            switch
            {
                Bar _ => true,
                _ => false
            };
        }

        public object ConvertToNative(Base @object)
        {
            switch (@object)
            {
                case IAppBeam o:
                    Report.Log($"Created Beam");
                    return BeamToNative(o);
                default:
                    return null;
            }
        }

        public List<object> ConvertToNative(List<Base> objects) => objects.Select(ConvertToNative).ToList();       

        public Base ConvertToSpeckle(object @object)
        {
            Base returnObject = null;
            switch (@object)
            {
                case Bar o:
                    returnObject = BeamToSpeckle(o);
                    Report.Log($"Converted Beam {o.Id}");
                    break;
                default:
                    //Report.Log($"Skipped not supported type: {@object.GetType()}{GetElemInfo(@object)}");
                    returnObject = null;
                    break;
            }

            return returnObject;
        }

        public List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();


        public void SetContextDocument(object doc)
        {
            Project = (Project)doc;
            Report.Log($"Using document: {Project.projectPath}");
           //Report.Log($"Using units: {ModelUnits}");
        }

        public void SetContextObjects(List<ApplicationPlaceholderObject> objects) => ContextObjects = objects;       

        public void SetConverterSettings(object settings) 
        {
            throw new NotImplementedException("This converter does not have any settings.");
        }

        public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects) => PreviousContextObjects = objects;        
    }
}
