--=======================================
-- (c) copyright 2015, XimiGame, LTD
-- All Rights Reserved. 
--=======================================
-- filename:  converted.lua
-- author:    wuxinhao
-- descrip:   protobuf 转 lua table
--=======================================


local descriptor = require "descriptor"
local FieldDescriptor = descriptor.FieldDescriptor

local ipairs = ipairs
local pairs = pairs
local print = print
local table = table 
local string = string
local tostring = tostring
local type = type
local error = error
local assert = assert

module("converted")

local function isEmptyTable(tab)
    if type(tab) == "table" then
        local count = 0
        for k,v in pairs(tab) do
            count = count + 1
            break;
        end
        if count == 0 then
            return true
        end
    end
    return false
end

function protobuf2Table( message , msg_field)
    assert(message,"protobuf2Table the message is must not nil")

    local function field2value( field, value )
        local tab = {}

        if field.cpp_type == FieldDescriptor.CPPTYPE_MESSAGE then

            if field.label == FieldDescriptor.LABEL_REPEATED then
                --repeated
                for i,v in ipairs(value) do
                    tab[i] = protobuf2Table(v)
                end
            else
                tab = protobuf2Table(value)
            end
        else
            if field.label == FieldDescriptor.LABEL_REPEATED then
                --repeated
                for i,v in ipairs(value) do
                    table.insert(tab, v )
                end
            else
                tab = value
            end
        end

        return tab
    end
        
    local tab = {}
    for field,value in pairs(message._fields) do
        tab[field.name] = field2value(field,value)
    end

    if not isEmptyTable(tab) then
        return tab
    end

    return nil
end
