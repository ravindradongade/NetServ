using Common.Logging;
using NetServNodeEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetServHttpWrapper
{
    public class HttpWrapper
    {
        private readonly ILog log = LogManager.GetLogger(typeof(HttpWrapper));
        public async Task<T> DoHttpGet<T>(string uri, int timeout = 0)
        {
            T deserializedEntity = default(T);
            try
            {
                log.Debug("Do Http Get to uri: " + uri);
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result =
                        await httpClient.GetAsync(uri, CancellationTokens.HttpOperation.Token);
                    result.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        var content = await result.Content.ReadAsAsync<string>();
                        if (typeof(T) == typeof(string))
                        {
                            deserializedEntity = (T)Convert.ChangeType(content, typeof(T));
                        }
                        else
                        {
                            deserializedEntity = JsonConvert.DeserializeObject<T>(content);
                        }
                        return deserializedEntity;
                    }

                    return deserializedEntity;

                }
            }

            catch (Exception ex)
            {
                log.Error(ex);
                return deserializedEntity;
            }

        }

        public async Task<bool> DoHttpPost<T>(string uri, T value, int timeout = 0)
        {
            try
            {
                log.Debug("Do Http post (return bool) to uri: " + uri);
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result = await httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);
                    result.EnsureSuccessStatusCode();
                    return result.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        public void DoHttpPostWithNoReturn<T>(string uri, T value, int timeout = 0)
        {
            try
            {
                log.Debug("Do Http post (with no return) to uri: " + uri);
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        public async Task<T1> DoHttpPost<T1, T2>(string uri, T2 value, int timeout = 0)
        {
            try
            {
                log.Debug("Do Http post (return T) to uri: " + uri);
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result = await httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);
                    result.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsAsync<T1>(CancellationTokens.HttpOperation.Token);
                    }
                    else
                    {
                        return default(T1);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return default(T1);
            }
        }
    }
}
