# NoteGitSync
Multi-host Git synchronization of personal notes based on C #, designed to achieve automatic note synchronization between companies and PCs.

## 使用方式

1. 先在GitHub上创建一个仓库，然后在本地需要同步的文件夹下进行Git的初始化、设置远程仓库等操作，保证能连上远程仓库。
2. 如果仓库不为空，则先拉取仓库中的内容到本地。
3. 如果是本地运行`cs`代码，在运行之前先修改`App.config`中的配置信息。如果是下载的`zip`包，先解压文件，然后修改`NoteGitSync`文件中`note.dll.config`文件的配置信息。配置完成后，双击`note.exe`运行即可。
4. 运行后就可以自动提交指定文件夹下的修改到远程仓库了。
