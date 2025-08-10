using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class ZenyBridgeClient : MonoBehaviour
{
    public string baseUrl = "http://localhost:3002";
    public IEnumerator Withdraw(string to, decimal amount)
    {
        var url = baseUrl + "/withdraw";
        var payload = "{\"to\":\""+to+"\",\"amount\":\""+amount+"\"}";
        using(var req = new UnityWebRequest(url,"POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type","application/json");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) Debug.LogError(req.error);
            else Debug.Log("Withdraw requested: "+req.downloadHandler.text);
        }
    }
}