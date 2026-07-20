using System;
using System.Globalization;
using System.Web.Mvc;

namespace MediCore.Models
{
    /// <summary>
    /// Corrige el conflicto entre la cultura del servidor (es-CR, formato dd/MM/yyyy,
    /// ver Web.config) y los inputs HTML5 &lt;input type="date"&gt;, que siempre envían
    /// el valor en formato ISO (yyyy-MM-dd) sin importar la cultura configurada.
    /// Sin este binder, el ModelBinder por defecto intenta parsear "yyyy-MM-dd" con la
    /// cultura es-CR, falla silenciosamente para tipos de valor no-nulos y termina
    /// guardando 01/01/0001 en la base de datos.
    /// </summary>
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
