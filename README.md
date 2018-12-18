# nlogrotate
简易滚动日志.net版

## 配置 logrotate
>配置文件内容格式如下：
>```nginx
>[directory]\[logfile] {
>  minutely|hourly|daily|weekly|monthly|yearly  [options]
>  [rotate [count]]
>  [compress]
>  [delaycompress [count]]
>  [includesubdirs]
>}
>
>```

> **配置文件加载位置**
> 1. `logrotate` 文件
> 2. `logrotate.d` 文件夹中的所有文件

> ### 配置详解
> #### 日志源 **directory**\\**logfile** 
>> **directory** 
>> 指定日志文件目录，默认使用当前logrotate.exe所在目录，可使用**.**或**..**格式的相对目录
>>
>> **logfile**
>> 指定日志文件名格式，默认 `*.log`，其中`*`代表任意字符
>>
> #### 触发选项 minutely|hourly|daily|weekly|monthly|yearly
>> **minutely** 每分钟触发，可在 options 参数指定触时刻(s)，默认`0`
>>
>> **hourly**   每小时触发，可在 options 参数指定触发时刻(m:s），默认`0:00`
>>
>> **daily**    每天触发，可在 options 参数指定触发时刻(H: m:s），默认`0:00:00`
>>
>> **weekly**   每周触发，可在 options 参数指定触发时刻(d H: m:s），默认`0 0:00:00`，其中 d 代表星期几（0代表周末）
>>
>> **monthly**  每月触发，可在 options 参数指定触发时刻(d H: m:s），默认`1 0:00:00`，其中 d 代表日期
>>
>> **yearly**   每年触发，可在 options 参数指定触发时刻(M-d H: m:s），默认`1-1 0:00:00`
>>
> #### 滚动选项 logrotate
>> 指定日志保留周期，参数 `count` 代表保留的滚动文件数（默认`90`），超期文件将被删除
>>
> #### 压缩选项 compress  delaycompress
>> **compress** 表示对滚动文件进行 gzip 压缩，若不需要压缩则不指定此选项
>>
>> **delaycompress** 指定压缩的延迟周期，默认1次
>>
> #### 子文件夹选项 includesubdirs
>> **includesubdirs** 指定此选项，滚动操作将包含目录中的子文件夹，默认不包含

## 管理logrotate
> ### logrotate 服务管理
> 通过logrotate.exe提供的命令 `--install`、`--start`、`--stop`、`--remove`对windows服务管理
>> #### 安装服务 
>>```cmd
>>logrotate.exe --install
>>```
>> #### 启用服务 
>>```cmd
>>logrotate.exe --start
>>```
>> #### 停止服务 
>>```cmd
>>logrotate.exe --stop
>>```
>> #### 卸载服务 
>>```cmd
>>logrotate.exe --remove
>>```
> ### logrotate 状态查看
> 通过 `logrotate.exe status` 查看运行状态
>
> ### logrotate 重新加载配置
> 通过 `logrotate.exe reload` 可将`logrotate`或`logrotate.d`中的配置重新加载

### 示例配置
```nginx
# 对 `logs`文件夹中所有 error开头的log文件进行滚动监视
logs\error*.log {
  # 在每分钟 30 秒触发滚动
  minutely  30
  # 保留60个滚动日志
  rotate 60
  # 启用压缩（未指定 delaycompress 默认延迟**1**周期压缩文件
  compress
}
```
