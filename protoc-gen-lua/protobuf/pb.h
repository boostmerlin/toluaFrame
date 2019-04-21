
#ifndef __LUA_PB_H_
#define __LUA_PB_H_

#if __cplusplus
extern "C" {
#endif
#include "lauxlib.h"
#include "lua.h"




	
int luaopen_pb(lua_State *L);


#if __cplusplus
}  /* end of the 'extern "C"' block */
#endif

#endif // __LUA_PB_H_
