import os

def doProtoLua():
    thisdir = os.path.dirname(os.path.realpath(__file__))
    protodir = os.path.join(thisdir,"proto")
    plugindir = os.path.join(thisdir,"plugin")

    protodirs = (protodir, os.path.join(protodir, "res"))
    
    batdir = os.path.join(plugindir,"protoc-gen-lua.bat")
    batfile = open(batdir, 'w')
    batfile.write("@python " + os.path.join(plugindir,"protoc-gen-lua"))
    batfile.close()
    
    luadir = os.path.join(thisdir,"lua")
    if not os.path.isdir(luadir):
        os.mkdir(luadir)
    
    for pbdir in protodirs:
        os.chdir(pbdir)
        plugbat = os.path.join(plugindir,"protoc-gen-lua.bat")
        protoc = os.path.join(thisdir,"protoc.exe")
        for filedir in os.listdir(pbdir):
            if not filedir.startswith('.') and filedir.endswith('.proto') and os.path.isfile(os.path.join(pbdir, filedir)):
                print("process proto: " + filedir)
                cmd = '%s --plugin=protoc-gen-lua="%s" --lua_out=%s %s' % (protoc, plugbat, luadir, filedir)
                os.system( cmd )

if __name__ == "__main__":
    import sys
    print(sys.version)
    doProtoLua()
