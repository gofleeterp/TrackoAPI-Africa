using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.Owin;

namespace TrackoApi.OData
{
    public class NullEntityTypeSerializer : ODataEntityTypeSerializer
    {
        public NullEntityTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        { }

        public override void WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (graph != null)
            {
                base.WriteObjectInline(graph, expectedType, writer, writeContext);
            }
        }
    }
    public class NullSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly NullEntityTypeSerializer _nullEntityTypeSerializer;

        public NullSerializerProvider()
        {
            _nullEntityTypeSerializer = new NullEntityTypeSerializer(this);
        }

        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
        {
            var serializer = base.GetODataPayloadSerializer(model, type, request);
            if (serializer == null)
            {
                var functions = model.SchemaElements.Where(s => s.SchemaElementKind == EdmSchemaElementKind.Function
                                                                || s.SchemaElementKind == EdmSchemaElementKind.Action);
                var isFunctionCall = functions.Select(f => $"{f.Namespace}.{f.Name}").Any(fname => request.RequestUri.OriginalString.Contains(fname));

                // only, if it is not a function call
                if (!isFunctionCall)
                {
                    var response = request.GetOwinContext().Response;
                    response.OnSendingHeaders(state =>
                    {
                        ((IOwinResponse)state).StatusCode = (int)HttpStatusCode.NotFound;
                    }, response);

                    // in case you are NOT using Owin, uncomment the following and comment everything above
                    // HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return _nullEntityTypeSerializer;
            }
            return serializer;
        }
    }
}
