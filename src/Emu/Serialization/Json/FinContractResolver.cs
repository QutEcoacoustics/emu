// <copyright file="FinContractResolver.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LanguageExt;
    using MoreLinq.Extensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Hopefully this is temporary until https://github.com/louthy/language-ext/issues/1230
    /// is resolved.
    /// </summary>
    public class FinContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType.IsGenericType
                && !objectType.IsGenericTypeDefinition
                && objectType.GetGenericTypeDefinition() == typeof(Fin<>))
            {
                var contract = base.CreateContract(objectType);
                var finTypeParameter = objectType.GetGenericArguments().First();
                contract.Converter = (JsonConverter)Activator.CreateInstance(typeof(FinJsonConverter<>).MakeGenericType(finTypeParameter));

                return contract;
            }

            return base.CreateContract(objectType);
        }
    }
}
