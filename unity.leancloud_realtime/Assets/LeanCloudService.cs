﻿// LeanCloudService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCloud;
using LeanCloud.Realtime;
// 引入 LeanCloud SDK
using UnityEngine;

public class LeanCloudService : MonoBehaviour {
  [SerializeField] private string appId; // LeanCloud AppId, 在控制台中获取
  [SerializeField] private string appKey; // LeanCloud AppKey, 在控制台中获取
  [SerializeField] private string clientId; // Realtime 通讯中一个用户的标识, 一般为用户的 id

  private static LeanCloudService instance;
  private AVRealtime realtime;
  private AVIMClient client;
  private readonly Dictionary<string, List<AVIMTextMessage>> _convMessages = new Dictionary<string, List<AVIMTextMessage>>();

  public static LeanCloudService Instance => instance;

  void Start() {
    // Singleton
    instance = this;
    DontDestroyOnLoad(this);
    // IMPORTANT: LeanCloud 的存储 SDK 是必须要初始化的
    AVClient.Initialize(appId, appKey);
    // IMPORTANT: 初始化 LeanCloud Realtime
    realtime = new AVRealtime(appId, appKey);
    // OPTIONAL: 开启调试日志
    if (Debug.isDebugBuild) {
      AVClient.HttpLog(Debug.Log);
      AVRealtime.WebSocketLog(Debug.Log);
    }
  }

  public async Task Login() {
    // IMPORTANT: 为用户创建一个 Client, 发送消息, 接收消息都是基于这个 Client 的
    client = await realtime.CreateClientAsync(clientId);
    // IMPORTANT: Register Message Received Callback
    client.OnMessageReceived += OnMessageReceived;
  }

  public List<AVIMTextMessage> GetMessages(AVIMConversation conv) {
    if (!_convMessages.ContainsKey(conv.ConversationId)) return new List<AVIMTextMessage>();
    return _convMessages[conv.ConversationId];
  }

  public async Task<AVIMConversation> CreateConversation(List<string> members, string cname, bool isUnique = true) {
    return await client.CreateConversationAsync(
      members: members,
      name: cname,
      isUnique: isUnique
    );
  }

  public async Task SendMessage(AVIMConversation conv, string content) {
    var message = new AVIMTextMessage(content);
    await conv.SendMessageAsync(message);
    if (!_convMessages.ContainsKey(conv.ConversationId)) {
      _convMessages[conv.ConversationId] = new List<AVIMTextMessage>();
    }

    _convMessages[conv.ConversationId].Add(message);
  }

  private void OnMessageReceived(object sender, AVIMMessageEventArgs e) {
    // Client 接收到新消息的回调函数
    if (e.Message is AVIMTextMessage) {
      // 目前只考虑 Text 类型的消息
      var textMessage = (AVIMTextMessage) e.Message;

      // 将各个 Conversation 的消息分别存储
      if (!_convMessages.ContainsKey(textMessage.ConversationId)) {
        _convMessages[textMessage.ConversationId] = new List<AVIMTextMessage>();
      }

      _convMessages[textMessage.ConversationId].Add(textMessage);
    }
  }
}