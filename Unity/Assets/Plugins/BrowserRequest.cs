using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Newtonsoft.Json;
using UnityEngine;

public class BrowserRequest
{
    private static Dictionary<int, string> responseMap = new Dictionary<int, string>();
    private static int requestIdCounter = 0;
    private const string baseUrl = "http://localhost";
    private static IntPtr callbackPtr;

    [System.Serializable]
    public class ServerResponse
    {
        public int status;
        public string body;
    }

    static BrowserRequest()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        callbackPtr = Marshal.GetFunctionPointerForDelegate((Action<IntPtr, int>)onResponseReceived);
#endif
    }

    [DllImport("__Internal")]
    private static extern void SendRequest(string method, string url, string bodyJson, int requestId, IntPtr callbackPtr);

    [DllImport("__Internal")]
    private static extern void FreeBuffer(IntPtr buffer);

    [MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
    public static void onResponseReceived(IntPtr dataPtr, int requestId)
    {
        string response = Marshal.PtrToStringUTF8(dataPtr);
        Debug.Log($"response: {response}\nfor requestId: {requestId}");
        FreeBuffer(dataPtr);

        responseMap[requestId] = response;
    }

    public int StartRequest(string method, string url, string bodyJson = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        int requestId = requestIdCounter++;
        Debug.Log($"request with {requestId}");
        SendRequest(method, baseUrl + url, bodyJson, requestId, callbackPtr);
        return requestId;
#else
        Debug.Log("Not running on webGL");
        return -1;
#endif
    }

    //해당 request에 대해 response 존재 시 반환 후 삭제, 없을 시 null
    public ServerResponse getResponse(int requestId)
    {
        if (responseMap.TryGetValue(requestId, out var response))
        {
            responseMap.Remove(requestId);
            ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response);
            return serverResponse;
        }
        else return null;
    }

    public IEnumerator waitForResponse(int requestId, float timeout, Action<ServerResponse> onResult)
    {
        float elapsed = 0f;
        ServerResponse response = null;
        while (response == null && elapsed < timeout)
        {
            response = getResponse(requestId);
            elapsed += Time.deltaTime;
            yield return null; //매 프레임 체크
        }
        if (response == null)
        {
            //timeout
            Debug.Log("timeout");
            onResult?.Invoke(null);
        }
        else
        {
            Debug.Log($"got response: {response.status}\n{response.body}");
            onResult?.Invoke(response);
        }
    }

}
