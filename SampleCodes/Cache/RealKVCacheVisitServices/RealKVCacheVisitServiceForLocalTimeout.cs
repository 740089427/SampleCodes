﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using SampleCodes.Serializer;
using SampleCodes.Thread;

namespace SampleCodes.Cache.RealKVCacheVisitServices
{
    /// <summary>
    /// 基于本地超时的KV缓存访问服务
    /// cacheConfiguration的格式为
    /// {
    ///     "MaxLength":最大缓存长度,
    ///     "ExpireSeconds":缓存过期时间
    /// }
    /// </summary>
    public class RealKVCacheVisitServiceForLocalTimeout : IRealKVCacheVisitService
    {
        private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static Dictionary<string, CacheContainer> _datas = new Dictionary<string, CacheContainer>();

        public async Task<V> Get<K, V>(string cacheConfiguration, Func<Task<V>> creator, string prefix, K key)
        {
            var configuration = JsonSerializerHelper.Deserialize<KVCacheConfiguration>(cacheConfiguration);
            if (!_datas.TryGetValue(prefix, out CacheContainer cacheContainer))
            {
                await _lock.WaitAsync();
                try
                {
                    if (!_datas.TryGetValue(prefix, out cacheContainer))
                    {
                        cacheContainer = new CacheContainer() { CacheDict = new HashLinkedCache<object, CacheTimeContainer<object>>() { Length = configuration.MaxLength } };
                        _datas[prefix] = cacheContainer;
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            CacheTimeContainer<object> cacheItem = cacheContainer.CacheDict.GetValue(key);
            if (cacheItem == null || cacheItem.Expire())
            {
                await cacheContainer.SyncOperate(
                async () =>
                {
                    cacheItem = cacheContainer.CacheDict.GetValue(key);
                    if (cacheItem == null || cacheItem.Expire())
                    {
                        var cacheValue = await creator();
                        cacheItem = new CacheTimeContainer<object>(cacheValue, configuration.ExpireSeconds);
                        cacheContainer.CacheDict.SetValue(key, cacheItem);
                    }
                }
                );

            }

            return (V)cacheItem.Value;
        }

        public V GetSync<K, V>(string cacheConfiguration, Func<V> creator, string prefix, K key)
        {
            var configuration = JsonSerializerHelper.Deserialize<KVCacheConfiguration>(cacheConfiguration);
            if (!_datas.TryGetValue(prefix, out CacheContainer cacheContainer))
            {
                _lock.Wait();
                try
                {
                    if (!_datas.TryGetValue(prefix, out cacheContainer))
                    {
                        cacheContainer = new CacheContainer() { CacheDict = new HashLinkedCache<object, CacheTimeContainer<object>>() { Length = configuration.MaxLength } };
                        _datas[prefix] = cacheContainer;
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            CacheTimeContainer<object> cacheItem = cacheContainer.CacheDict.GetValue(key);
            if (cacheItem == null || cacheItem.Expire())
            {
                cacheContainer.SyncOperate(
                () =>
                {
                    cacheItem = cacheContainer.CacheDict.GetValue(key);
                    if (cacheItem == null || cacheItem.Expire())
                    {
                        var cacheValue = creator();
                        cacheItem = new CacheTimeContainer<object>(cacheValue, configuration.ExpireSeconds);
                        cacheContainer.CacheDict.SetValue(key, cacheItem);
                    }
                }
               );

            }

            return (V)cacheItem.Value;
        }


        /// <summary>
        ///内部缓存容器
        ///提供线程同步处理方法
        /// </summary>
        private class CacheContainer
        {
            private LocalSemaphore _lock = new LocalSemaphore(1, 1);
            /// <summary>
            /// 缓存哈希链表存储
            /// 默认采用LRU（最近最久未访问）策略算法
            /// </summary>
            public HashLinkedCache<object, CacheTimeContainer<object>> CacheDict { get; set; }

            public async Task SyncOperate(Func<Task> action)
            {
                await _lock.SyncOperator(
                    async()=>
                    {
                        await action();
                    }
                    );
            }

            public void SyncOperate(Action action)
            {
                 _lock.SyncOperator(
                     () =>
                    {
                         action();
                    }
                    );
            }
        }


        /// <summary>
        /// 时间缓存容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class CacheTimeContainer<T>
        {
            private DateTime _cacheTime = DateTime.UtcNow;
            private int _timeout;

            public CacheTimeContainer(T value, int timeout)
            {
                Value = value;
                _timeout = timeout;
            }

            public T Value
            {
                get;
                set;
            }

            public bool Expire()
            {
                if (_timeout < 0)
                {
                    return false;
                }
                if ((DateTime.UtcNow - _cacheTime).TotalSeconds > _timeout)
                {
                    return true;
                }
                return false;
            }

        }


        /// <summary>
        /// 配置信息
        /// </summary>
        [DataContract]
        private class KVCacheConfiguration
        {
            /// <summary>
            /// 最大存储长度
            /// </summary>
            [DataMember]
            public int MaxLength { get; set; }
            /// <summary>
            /// 过期时间（单位秒）
            /// </summary>
            [DataMember]
            public int ExpireSeconds { get; set; }
        }
    }
}
