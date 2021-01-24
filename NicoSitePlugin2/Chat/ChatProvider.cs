﻿using Common;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NicoSitePlugin.Chat
{
    class ChatProvider
    {
        IChatOptions _chatOptions;
        Websocket _ws;
        public event EventHandler<IChatMessage> Received;
        public async Task ReceiveAsync(IChatOptions chatOptions)
        {
            _chatOptions = chatOptions;
            if (_ws != null)
            {
                throw new InvalidOperationException("_ws is not null");
            }
            _ws = new Websocket
            {
                //AutoSendPingInterval = 1000,
                //EnableAutoSendPing = true,
                NoDelay = true,
                SubProtocol = "msg.nicovideo.jp#json",
                Origin = "https://live2.nicovideo.jp",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36"

            };
            _ws.Received += Ws_Received;
            _ws.Opened += Ws_Opened;
            var url = "wss://msgd.live2.nicovideo.jp/websocket";
            try
            {
                await _ws.ReceiveAsync(url);
            }
            finally
            {
                _ws.Received -= Ws_Received;
                _ws.Opened -= Ws_Opened;
            }
        }
        public void SendPing()
        {
            _ws.Send("");
        }

        private void Ws_Opened(object sender, EventArgs e)
        {
            string s;
            if (_chatOptions is ChatLoggedInOptions loggedIn)
            {
                //[{"ping":{"content":"rs:0"}},{"ping":{"content":"ps:0"}},{"thread":{"thread":"M.rxo0XVWAqQOVwWJTPM_jsQ","version":"20061206","user_id":"123456","res_from":-150,"with_global":1,"scores":1,"nicoru":0,"threadkey":"T.h9hFdvpLaGczFTGN5NmMwnbE8EgC0t6uTD_ILf4XQJtNyBWc1ZtizVT7"}},{"ping":{"content":"pf:0"}},{"ping":{"content":"rf:0"}}]
                s = $"[{{\"ping\":{{\"content\":\"rs:0\"}}}},{{\"ping\":{{\"content\":\"ps:0\"}}}},{{\"thread\":{{\"thread\":\"{loggedIn.Thread}\",\"version\":\"20061206\",\"user_id\":\"2297426\",\"res_from\":-150,\"with_global\":1,\"scores\":1,\"nicoru\":0,\"threadkey\":\"{loggedIn.ThreadKey}\"}}}},{{\"ping\":{{\"content\":\"pf:0\"}}}},{{\"ping\":{{\"content\":\"rf:0\"}}}}]";
            }
            else if (_chatOptions is ChatGuestOptions guest)
            {
                s = $"";
            }
            else
            {
                //ここに来たら実装し忘れ。
                //_logger
                s = $"";
            }
            _ws?.Send(s);
        }

        private void Ws_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            var message = ChatParser.Parse(raw);
            Received?.Invoke(this, message);
        }

        internal void Disconnect()
        {
            _ws?.Disconnect();
        }
    }
}
