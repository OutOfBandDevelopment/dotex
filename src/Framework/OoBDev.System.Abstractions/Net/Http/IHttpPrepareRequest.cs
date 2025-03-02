using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Http;

public interface IHttpPrepareRequest
{
    void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
}
