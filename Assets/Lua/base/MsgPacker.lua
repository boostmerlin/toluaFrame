
local game_msg_pb_root = {}

local MsgPacker = {}
local this = MsgPacker

local protobuf2Table

function MsgPacker.init( )
    this.msgIds = {}
    -- for i,v in ipairs(game_msg_pb_root.PBCSMSG.fields) do
    --     this.msgIds[v.name] = v.number
    -- end
end

local function protobuf2Table(data)
    local FieldDescriptor = require("descriptor").FieldDescriptor
    local function field2value( field, value )
        local tab = {nil, nil, nil, nil, nil, nil, nil}
        if field.cpp_type == FieldDescriptor.CPPTYPE_MESSAGE then
            if field.label == FieldDescriptor.LABEL_REPEATED then
                for i,v in ipairs(value) do
                    tab[i] = protobuf2Table(v)
                end
            else
                tab = protobuf2Table(value)
            end
        else
            if field.label == FieldDescriptor.LABEL_REPEATED then
                for i,v in ipairs(value) do
                    table.insert(tab, v )
                end
            else
                tab = value
            end
        end
        return tab
    end
        
    local result = {nil, nil, nil, nil, nil, nil, nil}
    for field, value in pairs(data._fields) do
        result[field.name] = field2value(field, value)
    end

    return result
end

-- 格式：
-- | int , char , char                                  | int , binary                                 | int , binary                                 |
-- | 包长度（不包含自身int的四字节）, 两个标志位"X","X" | protobuf包头数据长度，protobuf包头二进制数据 | protobuf包体数据长度，protobuf包体二进制数据 |
-- @param table type. 为额外参数
-- msg should be type.
function MsgPacker.PackMsg(msgName, msg, param)
    local intLen = 4
    local tagLen = 2
    local data = ByteBuffer.New()
    
    local headBytes, headLen = this._buildHead(msgName, param)
    -- protobuf包体

    local bodyBytes = msg:SerializeToString()
    local bodyLen = #bodyBytes
    local totalLen = tagLen + intLen * 2 + headLen + bodyLen
    data:WriteInt(totalLen)
    data:WriteByte(88)
    data:WriteByte(88)			  -- tag
    --data:WriteInt(headLen)
    data:WriteBuffer(headBytes)
    --data:WriteInt(bodyLen)
    data:WriteBuffer(bodyBytes)

	return data
end

function MsgPacker.UnpackMsg(buffer)
    local pbHead = game_msg_pb_root.PBHead()
    pbHead:ParseFromString(buffer:ReadBuffer())
    local pbMsg = game_msg_pb_root.PBCSMsg()
    pbMsg:ParseFromString(buffer:ReadBuffer())

    return protobuf2Table(pbHead), protobuf2Table(pbMsg)
end

function MsgPacker._buildHead(msgName, param)
    local msg = game_msg_pb_root.PBHead()

    msg.main_version = 1
    msg.sub_version = 1
    msg.imei = "111234567"
    msg.cmd = this.msgIds[msgName]
    msg.channel_id = 8
    msg.device_name = AppUtil.deviceModel
    msg.device_id = "ssssss"
    msg.band = "undefined"
    msg.proto_version = 1

    if param  then
        if param.json_msg_id then
            msg.json_msg_id = param.json_msg_id
        end
        if param.json_msg then
            msg.json_msg = param.json_msg
        end 
    end    

    local data = msg:SerializeToString()
    return data, #data
end

return MsgPacker