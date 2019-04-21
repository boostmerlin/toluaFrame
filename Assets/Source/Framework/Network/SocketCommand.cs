using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using Ginkgo;

public class SocketCommand : Observer<SocketMessage> { 
    public override void OnNext(SocketMessage message) {
        object data = message.Body;
        if (data == null) return;
        KeyValuePair<int, ByteBuffer> buffer = (KeyValuePair<int, ByteBuffer>)data;
        switch (buffer.Key) {
            default: CSUtil.CallMethod("Network", "OnSocket", buffer.Key, buffer.Value); break;
        }
	}
}
