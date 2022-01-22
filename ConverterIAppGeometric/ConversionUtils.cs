using IA.Model;
using System.Linq;

using static IA.BLL.BLL_Entities;

namespace Objects.Converter.IApp
{
    public partial class ConverterIAppGeometric
    {
        /// <summary>
        /// Returns, if found, the corresponding doc element.
        /// The doc object can be null if the user deleted it. 
        /// </summary>
        /// <param name="applicationId">Id of the application that originally created the element, in Revit it's the UniqueId</param>
        /// <returns>The element, if found, otherwise null</returns>
        public Entity GetExistingElementByApplicationId(string applicationId)
        {
            if (applicationId == null)
                return null;

            var @ref = PreviousContextObjects.FirstOrDefault(o => o.applicationId == applicationId);

            if (@ref == null)
            {
                //element was not cached in a PreviousContex but might exist in the model
                //eg: user sends some objects, moves them, receives them 
                return GetComponentByGuid(Project.Building, applicationId);
            }

            //return the cached object, if it's still in the model
            return GetComponentByGuid(Project.Building, @ref.ApplicationGeneratedId);            
        }
    }
}
