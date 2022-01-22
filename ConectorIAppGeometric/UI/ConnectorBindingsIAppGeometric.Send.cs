using DesktopUI2.Models;
using DesktopUI2.ViewModels;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IA.BLL.BLL_Entities;
using Objects.Converter.IApp;

namespace Speckle.ConnectorIAppGeometric
{
    public partial class ConnectorBindingsIAppGeometric
    {
        #region sending
        public override async Task SendStream(StreamState state, ProgressViewModel progress)
        {
            var kit = KitManager.GetDefaultKit();


            var folder = KitManager.KitsFolder;

            var converter = kit.LoadConverter("AppGeometric");

           

            //var converter = new ConverterIAppGeometric(Project);

            converter.SetContextDocument(Project);
            Exceptions.Clear();           

            if (state.Filter != null)
            {
                state.SelectedObjectIds = GetObjectsFromFilter(state.Filter);
            }

            // remove deleted object ids
            var deletedElements = new List<string>();
            foreach (var Id in state.SelectedObjectIds)
            {
                if(GetComponentByGuid(Project.Building, Id) == null) deletedElements.Add(Id);
            }
            state.SelectedObjectIds = state.SelectedObjectIds.Where(o => !deletedElements.Contains(o)).ToList();

            if (state.SelectedObjectIds.Count == 0)
            {
                progress.Report.LogOperationError(new Exception("Zero objects selected; send stopped. Please select some objects, or check that your filter can actually select something."));
                return;
            }

            var commitObj = new Base();

            var conversionProgressDict = new ConcurrentDictionary<string, int>();
            conversionProgressDict["Conversion"] = 0;
            progress.Update(conversionProgressDict);

            foreach (var applicationId in state.SelectedObjectIds)
            {
                if (progress.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                //Base converted = null;
                //string containerName = string.Empty;

                //var typeAndName = ConnectorETABSUtils.ObjectIDsTypesAndNames
                //   .Where(pair => pair.Key == applicationId)
                //   .Select(pair => pair.Value).FirstOrDefault();

                //converted = converter.ConvertToSpeckle(typeAndName);

                //if (converted == null)
                //{
                //    var exception = new Exception($"Failed to convert object ${applicationId} of type ${selectedObjectType}.");
                //    progress.Report.LogConversionError(exception);
                //    continue;
                //}


                conversionProgressDict["Conversion"]++;
                progress.Update(conversionProgressDict);
            }



            return;
        }
        #endregion
    }
}
