using DesktopUI2;
using DesktopUI2.Models;
using IA.Model;
using System;
using System.Collections.Generic;

namespace Speckle.ConnectorIAppGeometric
{
    public partial class ConnectorBindingsIAppGeometric : ConnectorBindings
    {
        private static Project Project { get; set; }
        public List<Exception> Exceptions { get; set; } = new List<Exception>();
        public ConnectorBindingsIAppGeometric(Project project) => Project = project;       

        #region boilerplate
        public override string GetActiveViewName() => "Entire Building"; // Note: IApp does not have views that filter objects.        

        public override List<MenuItem> GetCustomStreamMenuItems() => new List<MenuItem>();

        public override string GetDocumentId() => Core.Models.Utilities.hashString(Project?.projectPath + Project?.projectName, Core.Models.Utilities.HashingFuctions.MD5);

        public override string GetDocumentLocation() => Project?.projectPath;

        public override string GetFileName() => Project?.projectName;

        public override string GetHostAppName() => ConnectorIAppGeometricUtils.IngeniumAppName;

        public override List<string> GetObjectsInView() => throw new NotImplementedException();
        #endregion    
    }
}
