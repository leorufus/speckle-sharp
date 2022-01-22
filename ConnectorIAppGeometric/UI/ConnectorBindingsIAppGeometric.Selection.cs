using DesktopUI2.Models.Filters;
using System;
using System.Collections.Generic;

using static IA.BLL.BLL_Entities;

namespace Speckle.ConnectorIAppGeometric
{
    public partial class ConnectorBindingsIAppGeometric
    {
        #region selection
        public override List<string> GetSelectedObjects()
        {
            throw new NotImplementedException();
        }

        public override List<ISelectionFilter> GetSelectionFilters()
        {
              return new List<ISelectionFilter>
              {        
                new AllSelectionFilter {Slug="all",  Name = "All", Icon = "CubeScan", Description = "Selects all project information." }
              };
        }

        public override void SelectClientObjects(string args)
        {
            throw new NotImplementedException();
        }

        private List<string> GetObjectsFromFilter(ISelectionFilter filter)
        {
            var objs = new List<string>();

            switch (filter.Slug)
            {
                case "all":
                    objs = GetBuildingComponentIDs(Project.Building);
                    break;               
            }

            return objs;
        }


        #endregion
    }
}
