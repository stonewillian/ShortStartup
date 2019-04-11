# C#
这是一个桌面快捷工具,支持自定义命令

1.如果想在界面增加一个快捷键,示例如下:
		<button content="VS2015" enable="true" program="E:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe" args="" workdirectory="" />

2.如果想支持含参数的脚本,示例如下:
		<button content="关机" enable="true" program="shutdown" args=" -s -f" workdirectory="" />

3.如果想指定工作空间,示例如下:
		<button content="GIT Python" enable="true" program="E:\Git\git-cmd.exe" args="" workdirectory="F:\Git\pythonGit" />

4.如果不想把所有按钮放一起,可以通过<group>的方式分TAB页:
	<group name="开发语言">
		<button content="PYTHON" enable="true" program="E:\Python\JetBrains\PyCharm Community Edition 2017.2\bin\pycharm64.exe" args="" workdirectory="" />
		<button content="VS2015" enable="true" program="E:\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe" args="" workdirectory="" />
	</group>
