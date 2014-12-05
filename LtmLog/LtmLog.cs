/*----------------------------------------------------------------------------------------------------
 * 
 *  	name 	:LtmLog
 *  	author 	:Liutangming
 *  	desc 	:Simple Log System
 * 
 * --------------------------------------------------------------------------------------------------*/

using System.IO;
using System.Collections.Generic;

namespace Ltm
{

	// 日志类型
	public enum LOG_TYPE
	{
		Unity = 1, 							// unity环境下日志类型
		Txt, 								// 录入文件日志类型
	}

	// 日志等级
	public enum LOG_LV
	{
		Usual = 1, 							// 普通
		Warning, 							// 警告
		Error, 								// 错误
	}

	public abstract class ILog
	{

		// 当前LOG对象是否有效
		protected bool _valid = true;

		// 当前LOG对象id
		protected string _id;

		// 当前LOG对象Type
		protected LOG_TYPE _type;

		// 是否同时开启文件写入，默认关闭
		protected bool _isFileIn = false;
		protected FileLog _fileLog = null;

		// 管理所有Log对象的集合
		protected static Dictionary<LOG_TYPE, Dictionary<string,ILog>> s_dictLogs = new Dictionary<LOG_TYPE, Dictionary<string,ILog>>();

		public string id
		{ get{return _id;} }
		public LOG_TYPE type
		{get{return _type;}}
		public bool valid
		{get{return _valid;}}
		public bool isFileIn
		{get{return _isFileIn;}}

		public ILog(LOG_TYPE typ, string id)
		{
			_type = typ;
			_id = id;
		}

		// 设置是否同时开启文件写入
		public void SetFileIn( bool bIn = true )
		{
			// 若本身就是文件日志类型，则直接返回
			if ( this.GetType() == typeof(FileLog) )
			{
				_isFileIn = false;
				return;
			}

			_isFileIn = bIn;

			if ( bIn && _fileLog == null )
			{
				_fileLog = new FileLog(id);
			}
		}


		// 设置是否有效
		public virtual void SetValid(bool valid = true)
		{
			_valid = valid;
		}


		// 添加一个日志对象到管理集合中
		protected static void Add(ILog log)
		{
			if ( log == null )
				return;

			if ( s_dictLogs.ContainsKey(log.type) == false )
				s_dictLogs[log.type] = new Dictionary<string, ILog>();

			if ( s_dictLogs[log.type].ContainsKey(log.id) == false )
				s_dictLogs[log.type].Add( log.id, log);
		}


		// 获取一个类型的日志对象，如果不存在，则创建
		public static ILog GetLog(LOG_TYPE typ, string id)
		{

			if ( s_dictLogs.ContainsKey(typ) && s_dictLogs[typ].ContainsKey(id) )
				return s_dictLogs[typ][id];

			ILog log = null;
			switch( typ )
			{
				case LOG_TYPE.Unity:
				{
#if UNITY_EDITOR
				 	log = new UnityLog(id);
				 	Add(log);	
#endif
				}
				break;

				case LOG_TYPE.Txt:
				{
					log = new FileLog(id);
					Add (log);
				}
				break;
			}

			return log;
		}


		// 静态的日志写入接口，返回一个日志对象
		public static ILog WriteLog(object msg, LOG_TYPE typ, string id, LOG_LV lv = LOG_LV.Usual, bool fileIn = false )
		{
			ILog log = GetLog( typ, id);
			if ( log == null )
				return null;

			log.SetValid(true);
			log.SetFileIn(fileIn);

			log.WriteLog(msg, lv);
			return log;
		}

		// 留给外部直接调用的日志录入接口
		public void WriteLog(object msg, LOG_LV lv = LOG_LV.Usual)
		{
			string msgLog = (string)msg;

			switch( lv )
			{
				case LOG_LV.Usual:
				{
				 	Log(msgLog);
				}
				break;

				case LOG_LV.Warning:
				{
				 	LogWarning(msgLog);
				}
				break;

				case LOG_LV.Error:
				{
				 	LogError(msgLog);
				}
				break;
			}


			// 如果同时开启了文件录入，则同时写入文件
			if ( _isFileIn && _fileLog != null )
			{
				_fileLog.WriteLog(msgLog);
			}
		}


		// 寄生类需要内部实现的录入接口
		protected abstract void Log(object obj);
		protected abstract void LogWarning(object obj);
		protected abstract void LogError(object obj);

	}


	// 用于Unity编辑器下的Log类型
	public class UnityLog : ILog
	{

		public UnityLog(string id) : base(LOG_TYPE.Unity, id)
		{
		}

		protected override void Log(object obj)
		{
#if UNITY_EDITOR
			if (valid)
				UnityEngine.Debug.Log(obj);
#endif
		}
		protected override void LogWarning(object obj)
		{
#if UNITY_EDITOR
			if (valid)
				UnityEngine.Debug.LogWarning(obj);
#endif
		}
		protected override void LogError(object obj)
		{
#if UNITY_EDITOR
			if (valid)
				UnityEngine.Debug.LogError(obj);
#endif
		}

	}




	// 写入文件LOG
	public class FileLog : ILog
	{

		protected StreamWriter _streamWriter = null;
		protected string _file = null;

		public FileLog(string id) : base( LOG_TYPE.Txt, id)
		{
			_file = GenFileName(id);
		}

		~FileLog()
		{
			Close();
		}

		// 生成文件名，同时如果不存在路径，则生成
		protected string GenFileName( string name, string ext = "txt")
		{
			string path = System.Environment.CurrentDirectory;
			int idx = path.LastIndexOf('\\');
			string newFile;
			string newPath;
			if ( idx > 0 )
			{
				newPath = path + "\\Log\\";
				newFile = newPath + name + "." + ext;
			}
			else
			{
				newPath = path + "Log\\" ;
				newFile = newPath + name + "." + ext;
			}

			if (Directory.Exists(newPath)== false)
			{
				Directory.CreateDirectory(newPath);
			}

			return newFile;
		}

		// 打开对应文件写入流
		protected bool Open()
		{
			if ( _streamWriter != null )
				return true;
			
			if ( !File.Exists(_file) )
			{
				_streamWriter = new StreamWriter(_file,true);
			}
			else
			{
				_streamWriter = new StreamWriter(_file, true);
			}

			return true;
		}

		protected bool Close()
		{
			if( _streamWriter != null )
			{
				_streamWriter.Close();
			}
	
			_streamWriter = null;

			return true;
		}


		protected override void Log(object obj)
		{
			if ( valid )
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
		}

		protected override void LogWarning(object obj)
		{
			if ( valid )
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
		}

		protected override void LogError(object obj)
		{
			if ( valid )
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
		}


	}




}
