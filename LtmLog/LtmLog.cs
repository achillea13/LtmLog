/*----------------------------------------------------------------------------------------------------
 * 
 *  	name 	:LtmLog
 *  	author 	:Liutangming
 *  	desc 	:Simple Log System
 * 
 * 
 *  	Interface 	:
 * 
 *  		初始化日志的类型
 *  		LtmLog:Init(LOG_TYPE typ) 	
 * 
 *  		判断指定类型是否存在环境
 *  		LtmLog:IsEnvSuit(LOG_TYPE typ)
 * 
 *  		信息写入日志
 *  		LtmLog:WriteLog(object msg, string id = null, LOG_LV lv = LOG_LV.Usual, FILE_IN fileIn = FILE_IN.Null )
 * 
 *  		禁用某个ID的日志系列
 * 		 	LtmLog:Disable(string id)
 * 
 *  		禁用所有的日志系列，不包括default系列
 *  		LtmLog:DisableAll()
 * 
 *  		启用某个ID的日志系列
 *  		LtmLog:Enable(string id)
 * 
 *  		启用所有的日志系列
 *  		LtmLog:EnableAll()
 * 
 * --------------------------------------------------------------------------------------------------*/

using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace Ltm
{

	// 日志类型
	public enum LOG_TYPE
	{
		Null = 0, 							// null
		Unity = 1, 							// unity环境下日志类型
		Txt, 								// 录入文件日志类型
		Console, 							// c#控制台输出
	}

	// 日志等级
	public enum LOG_LV
	{
		Usual = 1, 							// 普通
		Warning, 							// 警告
		Error, 								// 错误
	}

	// 返回枚举
	public enum ERR_CODE
	{
		OK = 0, 							// 正确
		Unknow, 							// 未知错误
		EnvNotExist, 						// 平台不匹配
		InitAlready, 						// 已经初始化
	}

	// 是否写入文件
	public enum FILE_IN
	{
		Null = 0, 								// 保持不变
		Yes, 								// 写入
		No, 								// 不写入
	}

	public static class LtmLog
	{


		// 是否已经初始化
		private static bool s_inited = false;

		// 是否开启日志的时间采集
		private static bool s_timeRecord = true;
	
		// 管理所有Log对象的集合
		private static Hashtable s_tabLogs = new Hashtable();

		// 管理所有默认的LOg对象
		private static Hashtable s_tabLogsDefault = new Hashtable();
	
		// 当前指定的LogType
		private static LOG_TYPE _type = LOG_TYPE.Txt;

		private static string GenKey( LOG_TYPE typ, string id)
		{
			return typ + "__" + id;
		}

		// 获取一个类型的日志对象，如果不存在，则创建
		private static ILog GenLog(LOG_TYPE typ, string id)
		{

			ILog log = null;
			switch( typ )
			{
				case LOG_TYPE.Unity:
				{
#if UNITY_EDITOR
				 	log = new UnityLog(id);
#endif
				}
				break;

				case LOG_TYPE.Txt:
				{
					log = new FileLog(id);
				}
				break;

				case LOG_TYPE.Console:
				{
				 	log = new ConsoleLog(id);
				}
				break;
			}

			return log;
		}

		// 指定一些参数
		public static ERR_CODE Init( LOG_TYPE typ, bool timeRecord = true )
		{
			if ( s_inited )
				return ERR_CODE.InitAlready;

			bool bEnvSuit = IsEnvSuit(typ);
			// 根据环境指定
			if ( bEnvSuit )
				_type = typ;
			else
				_type = LOG_TYPE.Txt;

			string key = GenKey(_type,"default");
			if ( s_tabLogsDefault.ContainsKey(key) == false )
			 	s_tabLogsDefault[key] = GenLog(_type,"default");

			s_timeRecord = timeRecord;

			s_inited = true;

			return bEnvSuit == true ? ERR_CODE.OK : ERR_CODE.EnvNotExist;

		}

		// 判断环境是否存在
		public static bool IsEnvSuit( LOG_TYPE typ )
		{
			switch( typ )
			{
				case LOG_TYPE.Unity:
				{
#if UNITY_EDITOR
				return true;

#else
				return false;
#endif

				}
				break;


			}

			return true;
		}


		// 静态的日志写入接口
		// fileIn 	: 是否写入文件
		public static void WriteLog(object msg, string id = null, LOG_LV lv = LOG_LV.Usual, FILE_IN fileIn = FILE_IN.Null )
		{
			ILog log = null;
			if ( id == null ) 
			{
				string key = GenKey( _type, "default");
				log = (ILog)s_tabLogsDefault[key];
			}
			else
			{
				string key = GenKey( _type, id);
				if ( s_tabLogs.ContainsKey(key) )
					log = (ILog)s_tabLogs[key];
				else
				{
					log = GenLog(_type, id);
					s_tabLogs[key] = log;
				}
			}

			switch( fileIn )
			{
				case FILE_IN.No:
				{
					log.SetFileIn(false);
				}
				break;
				case FILE_IN.Yes:
				{
					log.SetFileIn(true);
				}
				break;
	
			}

			log.WriteLogDo(MsgConstruct(msg, lv), lv);
			return;
		}


		private static string MsgConstruct( object msg, LOG_LV lv )
		{
			string res = "";
			string level = "";
			string ti = "";

			switch( lv )
			{
				case LOG_LV.Usual:
				{
				 	level = "[usual]";
				}
				break;

				case LOG_LV.Warning:
				{
				 	level = "[warning]";
				}
				break;

				case LOG_LV.Error:
				{
					level = "[error]";
				}
				break;
			}

			if ( s_timeRecord )
			{
				ti = GetLocalTimeStr();
			}

			res = level + "  " + msg + "          " + ti;

			return res;
		}

		private static string GetLocalTimeStr()
		{
			return System.DateTime.Now.ToString();
		}


		private static void Turn( string id, bool bTurn )
		{
			string key = GenKey(_type, id);
			if ( s_tabLogs.ContainsKey(key) && s_tabLogs[key] != null )
				((ILog)s_tabLogs[key]).SetValid(bTurn);
		}

		private static void TurnAll(bool bTurn)
		{
			foreach( DictionaryEntry item in s_tabLogs )
			{
				if ( item.Value != null && (ILog)(item.Value) != null )
					((ILog)(item.Value)).SetValid(bTurn);
			}
		}

		// disable log by id
		// if param id is not sig , the last log object will be del
		public static void Disable( string id)
		{
			Turn(id,false);
		}

		public static void DisableAll()
		{
			TurnAll(false);
		}

		public static void Enable( string id )
		{
			Turn(id,true);


		}

		public static void EnableAll()
		{
			TurnAll(true);
		}








/*---------------------------------------------------------------------------------------------------
 * -------------------------------  Base log class 		-------------------------------------------*/

		private abstract class ILog
		{
			// 当前LOG对象id
			public string _id;
			
			// 当前LOG对象Type
			public LOG_TYPE _type;

			// 是否同时开启文件写入，默认关闭
			public bool _isFileIn = false;
			protected FileLog _fileLog = null;

			protected bool _valid = true;


			// 是否激活
			public virtual void SetValid(bool valid)
			{
				_valid = valid;
			}


			public ILog(LOG_TYPE typ, string id)
			{
				_type = typ;
				_id = id;
			}

			// 设置是否同时开启文件写入
			public void SetFileIn( bool bIn = true )
			{
				// 若本身就是文件日志类型，则直接返回
				if ( _type == LOG_TYPE.Txt )
				{
					_isFileIn = false;
					return;
				}
				
				_isFileIn = bIn;
				
				if ( bIn && _fileLog == null )
				{
					_fileLog = new FileLog(_id);
				}
			}

			// 留给manager直接调用的日志录入接口
			public void WriteLogDo(object msg, LOG_LV lv = LOG_LV.Usual)
			{
				if (!_valid)
					return;

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
					_fileLog.WriteLogDo(msgLog);
				}
			}


			// 寄生类需要内部实现的录入接口
			protected abstract void Log(object obj);
			protected abstract void LogWarning(object obj);
			protected abstract void LogError(object obj);

		}


/* -------------------------------  Base log class end	---------------------------------------------
/*-------------------------------------------------------------------------------------------------*/



/*--------------------------------------------------------------------------------------------------
 * ------------------------------- 	Unity3D type log 	--------------------------------------------*/
		// 用于Unity编辑器下的Log类型
		private class UnityLog : ILog
		{
			
			public UnityLog(string id) : base(LOG_TYPE.Unity, id)
			{
			}
			
			protected override void Log(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.Log(obj);
				#endif
			}
			protected override void LogWarning(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.LogWarning(obj);
				#endif
			}
			protected override void LogError(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.LogError(obj);
				#endif
			}

		}
/*-------------------------------	Unity3D type log end --------------------------------------------
 * -------------------------------------------------------------------------------------------------*/



/*--------------------------------------------------------------------------------------------------
 * ------------------------------- 	Console type log 	--------------------------------------------*/
		// 用于Unity编辑器下的Log类型
		private class ConsoleLog : ILog
		{
			
			public ConsoleLog(string id) : base(LOG_TYPE.Console, id)
			{
			}
			
			protected override void Log(object obj)
			{
				System.Console.WriteLine(obj);
			}
			protected override void LogWarning(object obj)
			{
				System.Console.WriteLine(obj);
			}
			protected override void LogError(object obj)
			{
				System.Console.WriteLine(obj);
			}
		}
/*-------------------------------	Console type log end --------------------------------------------
 * -------------------------------------------------------------------------------------------------*/




/*--------------------------------------------------------------------------------------------------
 * ------------------------------- 	File type log 		--------------------------------------------*/
		// 写入文件LOG
		private class FileLog : ILog
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
				int idx = path.LastIndexOf(Path.DirectorySeparatorChar);

			
				string newFile;
				string newPath;
				if ( idx > 0 )
				{
					newPath = path +Path.DirectorySeparatorChar + "Log" +Path.DirectorySeparatorChar;
					newFile = newPath + name + "." + ext;
				}
				else
				{
					newPath = path + "Log" +Path.DirectorySeparatorChar ;
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
				
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			protected override void LogWarning(object obj)
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			protected override void LogError(object obj)
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			
		}
/*-------------------------------	File type log end 	--------------------------------------------
 * -------------------------------------------------------------------------------------------------*/


	}

}
