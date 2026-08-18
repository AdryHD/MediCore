using System;
using System.Globalization;
using System.Web.Mvc;

namespace MediCore.Models
{

    public class DateTimeModelBinder : IModelBinder
    {
        private static readonly string[] FormatosIso =
        {
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mm:ss"
        };

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valorProvisto = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valorProvisto == null)
            {
                return null;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valorProvisto);

            var rawValue = valorProvisto.AttemptedValue;
            bool esNullable = bindingContext.ModelType == typeof(DateTime?);

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return esNullable ? (object)null : default(DateTime);
            }

            DateTime fecha;

            if (DateTime.TryParseExact(rawValue, FormatosIso, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)
                || DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)
                || DateTime.TryParse(rawValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out fecha))
            {
                return fecha;
            }

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "El valor ingresado no es una fecha válida.");
            return esNullable ? (object)null : default(DateTime);
        }
    }
}