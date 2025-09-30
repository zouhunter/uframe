//
//  UTownBridge.m
//  
//
//  Created by supertext on 2022/1/25.
//

#import <Foundation/Foundation.h>

@interface UTBridgePlugin:NSObject
+(void)sendMethod:(NSDictionary *)dic;
+(void)callMethod:(NSDictionary *)dic;
@end

#ifdef __cplusplus
extern "C" {
#endif
   //unity通知原生调用
   typedef void (*MethodEvent) (const char *cmd,const char *params);
   MethodEvent _methodListenr;
   MatePlugin* _matePlugin = [[MatePlugin alloc] init];

   void unity_call(const char* cmd, const char* param) {
       NSLog(@"unity_call:%s, %s",cmd,param);
       [[NSNotificationCenter defaultCenter] postNotificationName:@"unityCalliOS" object:nil userInfo:@{@"cmd":[NSString stringWithUTF8String:cmd],@"param":[NSString stringWithUTF8String:param]}];
   }

   //unity注册原生调用
   void unity_regist_callback(MethodEvent nativeCallback){
       NSLog(@"unity_regist_callback");
       _methodListenr = nativeCallback; // 根据之前讨论，可能需要修改变量名
       [[NSNotificationCenter defaultCenter] addObserver:_matePlugin selector:@selector(call_unity:) name:@"iOSCallUnity" object:nil]; // 创建了 MatePlugin 的实例并订阅
   }
}


#ifdef __cplusplus
}
#endif

@implementation MatePlugin
- (void)call_unity:(NSNotification *)aNotification {
    NSDictionary *userInfo = [aNotification userInfo];

    NSString *cmd = userInfo[@"cmd"];
    NSString *param = userInfo[@"param"];
    if (_methodListenr != NULL){ // 根据之前讨论，可能需要修改变量名
        _methodListenr([cmd UTF8String], [param UTF8String]);
    } else {
        NSLog(@"call_unity, but _methodListenr is null!"); // 根据之前讨论，可能需要修改变量名
    }
}
@end