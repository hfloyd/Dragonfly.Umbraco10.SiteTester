namespace Dragonfly.UmbracoSiteTester.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Umbraco.Cms.Core;

    public enum ResultType
    {
        Ok,
        CheckPossibleInlineError,
        Redirected,
        ErrorRequestException,
        ErrorHttpOther,
        Error404,
        Error500,
        NotTestedProtected,
        NotTestedUnPublished,
        NotTestedExcludedByConfig,
        Unknown
    }
    public class RenderingResult
    {
        public int NodeId { get; set; }

        public Udi NodeUdi { get; set; }

        public string NodeName { get; set; }

        public string Url { get; set; }

        public string ContentTypeAlias { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType Result { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HttpStatusCode StatusCode { get; set; }

        public string StatusCodeMessage { get; set; }

        public IEnumerable<string> ContentErrorMatches { get; set; }

        public string RenderedOutput { get; set; }

        public LinksSet OnPageLinks { get; set; }


        #region Handle Exceptions for error-free deserialization

        private Exception? _exceptionObject;
        private SerializableException? _serializableException;

        /// <summary>
        /// Exception
        /// </summary>
        /// <remarks>Use ".SetException()" method </remarks>
        [JsonIgnore]
        public Exception? ErrorException
        {
            get { return _exceptionObject; }
        }

        /// <summary>
        /// Serializable Exception Info
        /// </summary>
        /// <remarks>Use ".SetException()" method </remarks>
        public SerializableException? ExceptionInfo
        {
            get
            {
                return _serializableException;
            }
        }

        /// <summary>
        /// Exception
        /// </summary>
        //[JsonIgnore]
        public void SetException(Exception? value)
        {
            _exceptionObject = value;

            if (value != null)
            {
                _serializableException = ConvertToSerializableException(value);
            }
        }

        private SerializableException ConvertToSerializableException(Exception Ex)
        {
            var info = new SerializableException();
            info.ClassName = Ex.GetType().ToString();
            info.Message = Ex.Message;
            info.Data = Ex.Data;
            info.StackTrace = Ex.StackTrace;
            info.HResult = Ex.HResult;
            info.Source = Ex.Source;
            
            if (Ex.InnerException != null)
            {
                info.InnerException= ConvertToSerializableException(Ex.InnerException);
            }

            return info;
        }
        
        #endregion
    }

    public class SerializableException
    {
        [JsonProperty("ClassName")]
        public string ClassName { get; set; } = "";

        [JsonProperty("Message")]
        public string Message { get; set; } = "";

        [JsonProperty("Data")]
        public IDictionary? Data { get; set; }

        [JsonProperty("StackTrace")]
        public string? StackTrace { get; set; }
        
        [JsonProperty("HResult")]
        public long? HResult { get; set; }

        [JsonProperty("Source")]
        public string? Source { get; set; }

        [JsonProperty("InnerException")]
        public SerializableException? InnerException { get; set; }
    }
}
