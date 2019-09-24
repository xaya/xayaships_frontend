if "%5"=="" ("%cd%\Assets\StreamingAssets\shipsd\ships-channel.exe" --xaya_rpc_url="%1" --gsp_rpc_url="%2" --broadcast_rpc_url="http://seeder.xaya.io:10042" --rpc_port="29060"  --playername="%4" --channelid="%3" -alsologtostderr  --v=1) else ("%cd%\Assets\StreamingAssets\shipsd\ships-channel.exe" --xaya_rpc_url="%1" --gsp_rpc_url="%2" --broadcast_rpc_url="http://seeder.xaya.io:10042" --rpc_port="29060"  --playername="%4 %5" --channelid="%3" -alsologtostderr  --v=1)
echo off
echo "Press any key"
PAUSE