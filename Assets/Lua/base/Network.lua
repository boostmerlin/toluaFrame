
local game_msg_root = {}

local MsgPacker = require "base/MsgPacker"
local MsgDispacher = require "base/MsgDispatcher"
local FieldDescriptor = require("protobuf.descriptor").FieldDescriptor
local LABEL_REPEATED = FieldDescriptor.LABEL_REPEATED
local CPPTYPE_MESSAGE = FieldDescriptor.CPPTYPE_MESSAGE

Network = {}
local this = Network
this.onNetState = nil
this.onNetRetryMax = nil

local isConnected = false
local MAX_RETRY = 3

local sendDataBuffer = {}
local recvMsgBuffer = {}
local connectTimer
local retryCount = 0
local isDisReconnect = false

local Protocal = {
    Connect		= 101,	--连接服务器
	Exception   = 102,	--异常掉线
	Disconnect  = 103,	--正常断线   
	Message		= 104,	--接收消息
}

function Network.Start()
    this.handleFunc = {
        [Protocal.Connect] = this.OnConnect,
        [Protocal.Exception] = this.OnException,
        [Protocal.Disconnect] = this.OnDisconnect,
        [Protocal.Message] = this.OnMessage,
    }
end

--Socket消息--
function Network.OnSocket(key, data)
    local f = this.handleFunc[key]
    if f then
        f(data)
    end
end

--当连接建立时--
function Network.OnConnect() 
    isConnected = true
    log("----------------------------Game Server connected!!")
    if this.onNetState then
        this.onNetState(isConnected)
    end
    if connectTimer then
        connectTimer:Stop()
        connectTimer = nil
    end
    retryCount = 0
    for _, v in ipairs(sendDataBuffer) do
        networkMgr:SendMessage(v)
    end
    sendDataBuffer = {}
end

function Network.IsConnected()
    return isConnected
end

--异常断线--
function Network.OnException() 
    logError("OnException------->>>>");
    isConnected = false;
    if this.onNetState then
        this.onNetState(isConnected)
    end
end

function Network.ConnectServer(isDisReconn)
    isConnected = false; 
    networkMgr:SendConnect();
end

--连接中断，或者被踢掉--
function Network.OnDisconnect() 
    isConnected = false;
    logError("OnDisconnect------->>>>");
    if this.onNetState then
        this.onNetState(isConnected)
    end
end

function Network.Close(manual) 
    isConnected = false
    log("Network.Close")
    if not manual then
        networkMgr:Close()
    else
        networkMgr:ManualDisconnect()
    end
end

--@param buffer ByteBuffer
--int pbHead int pbMsg
function Network.OnMessage(buffer)
    --消息的处理全放到Ctrl中
    log('OnMessage-------->>> pbHead:');
    local pbHead, pbMsg = MsgPacker.UnpackMsg(buffer)
    local k, _ = next(pbMsg, nil)
    logTable(pbHead)
    log('pbMsg: ', k, os.clock())
    logTable(pbMsg)

    SLUtil.SynchroServerTime(pbHead.cur_stamp)
    MsgDispacher.Dispatch(pbHead, k, pbMsg[k])
end

--卸载网络监听--
function Network.Unload()
    logWarn('Unload Network...');
end

local function mergeFromTable(msg, t)
    for k, v in pairs(t) do
        local childMsg = msg[k]
        --check default.
        local typeMsg = type(childMsg)
        if typeMsg == "table" then
            local fields = getmetatable(msg)._descriptor.fields
            local field
            for _, f in ipairs(fields) do
                if f.name == k then
                    field = f
                    break
                end
            end
            if not field then
                error("error message field name:" .. k)
            end
            if field.cpp_type == CPPTYPE_MESSAGE then
                if field.label == LABEL_REPEATED then
                    for _, vv in ipairs(v) do
                        local subChildMsg = childMsg:add()
                        mergeFromTable(subChildMsg, vv)
                    end
                else
                    mergeFromTable(childMsg, v)
                end
            else
                for _, vv in ipairs(v) do
                    childMsg:append(vv)
                end
            end
        else
            msg[k] = v
        end
    end
end

--[[
    @msg pb msg type.
    @param extra param, table table type.
]]
function Network.Send(msgName, param, headParam)
    if not param then
        error("Network.Send param must specified.")
    end

    local msg = game_msg_root.PBCSMsg()
    local subMsg = msg[msgName]
    if next(param) == nil then
        subMsg._is_present_in_parent = true
    end

    mergeFromTable(subMsg, param)

    log("Pre Send Message-----> ", msgName, os.clock())
    if param then
        logTable(param)
    end
    if headParam then
        logTable(headParam)
    end

    local data = MsgPacker.PackMsg(msgName, msg, headParam)

    if isConnected then
        networkMgr:SendMessage(data)
    else
        table.insert(sendDataBuffer, data)
        Network.ConnectServer()
    end
    --do not close, c# do.
    --data.Close
end

function Network.IsReconnect()
    return isDisReconnect
end

function Network.TryReconnect()
    if retryCount and retryCount > 0 then
        return false
    end
    isDisReconnect = true
    if connectTimer then
        connectTimer:Stop()
        connectTimer = nil
    end

    connectTimer = Timer.New(function ()
        retryCount = retryCount + 1
        if retryCount > MAX_RETRY then
            logError("retry reach max, Can't connected to server.")
            --TODO: notice
            if this.onNetRetryMax then
                this.onNetRetryMax()
            end
            return
        end
        log("----------------------------------TryReconnect times: ", retryCount)
        this.ConnectServer(true)
    end
    , 15, MAX_RETRY, true)
    connectTimer:Start()

    retryCount = 1
    log("----------------------------------TryReconnect times: ", retryCount)
    this.ConnectServer(true)

    return true
end