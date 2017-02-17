using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.HttpUtilities
{
    using System.Net;
    using System.Net.Http;

    using NetServeNodeEntity.Exceptions;

    using NetServNodeEntity;

    using Newtonsoft.Json;

    internal class HttpWrapper
    {
        internal async Task<T> DoHttpGet<T>(string uri, int timeout=0)
        {
            T deserializedEntity = default(T);
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result =
                        await httpClient.GetAsync(uri, CancellationTokens.HttpOperation.Token);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = await result.Content.ReadAsAsync<string>();
                        if (typeof(T) == typeof(string)|| typeof(T) == typeof(int))
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
                return deserializedEntity;
            }

        }

        internal async Task<bool> DoHttpPost<T>(string uri, T value,int timeout=0)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result = await httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);
                    return result.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {

                return false;
            }
        }

        internal void DoHttpPostWithNoReturn<T>(string uri, T value, int timeout = 0)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);
                   
                }
            }
            catch (Exception)
            {

               
            }
        }
        internal async Task<T1> DoHttpPost<T1, T2>(string uri, T2 value,int timeout=0)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    if (timeout > 0)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    }
                    var result = await httpClient.PostAsJsonAsync(uri, value, CancellationTokens.HttpOperation.Token);
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
            catch (Exception)
            {

                return default(T1);
            }
        }
    }
}
