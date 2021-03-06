﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SampleCodes.Cache;
using SampleCodes.Cache.RealKVCacheVisitServices;
using SampleCodes.Cache.RealKVCacheVisitServices.KVCacheVersionServices;
using SampleCodes.Thread;

namespace SampleCodes
{
    class Program
    {
        async static Task Main(string[] args)
        {

            





            //初始化
            Init();


            await LocalTimeout();

            await LocalVersion();

            await Combination();

        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Init()
        {
            KVCacheVisitorIMP.RealKVCacheVisitServiceFactories["LocalTimeout"] = RealKVCacheVisitServiceForLocalTimeoutFactory.GetFactory();
            KVCacheVisitorIMP.RealKVCacheVisitServiceFactories["LocalVersion"] = RealKVCacheVisitServiceForLocalVersionFactory.GetFactory();
            KVCacheVisitorIMP.RealKVCacheVisitServiceFactories["Combination"] = RealKVCacheVisitServiceForCombinationFactory.GetFactory();

            RealKVCacheVisitServiceForLocalVersion.KVCacheVersionServiceFactories["Test"] = KVCacheVersionServiceForTestFactory.GetFactory();

            KVCacheVisitorRepository.Datas["Cache1"]= new KVCacheVisitor()
            {
                Name = "Cache1",
                CacheType = "LocalTimeout",
                CacheConfiguration = @"{
                                        ""MaxLength"":2,
                                        ""ExpireSeconds"":-1
                                       }"
            };

            KVCacheVisitorRepository.Datas["Cache2"] = new KVCacheVisitor()
            {
                Name = "Cache2",
                CacheType = "LocalVersion",
                CacheConfiguration = @"{
                                        ""MaxLength"":3,
                                        ""VersionCallTimeout"":5,
                                        ""VersionNameMappings"":
                                            {
                                                ""System.String-System.String"":""Test""
                                            },
                                         ""DefaultVersionName"":""Test""
                                       }"
            };

            KVCacheVisitorRepository.Datas["Cache3"] = new KVCacheVisitor()
            {
                Name = "Cache3",
                CacheType = "Combination",
                CacheConfiguration = @"{
                                        ""VistorNames"":[""Cache1"",""Cache2""]
                                       }"
            };


        }

        private static async Task LocalTimeout()
        {

            var cache=await KVCacheVisitorRepositoryFactory.Get().QueryByName("Cache1");

            for(var index = 0;index<=15;index++)
            {
                //创建key1，key2,key3,三个缓存
                var cacheValue = await cache.Get<string, string>(
                     async (k) =>
                     {
                         return await Task.FromResult(Guid.NewGuid().ToString());
                     }, "Key1");

                Console.WriteLine($"Key1:{cacheValue}");

                cacheValue = await cache.Get<string, string>(
                                async (k) =>
                                {
                                    return await Task.FromResult(Guid.NewGuid().ToString());
                                }, "Key2");

                Console.WriteLine($"Key2:{cacheValue}");

                cacheValue = await cache.Get<string, string>(
                async (k) =>
                {
                    return await Task.FromResult(Guid.NewGuid().ToString());
                }, "Key3");

                Console.WriteLine($"Key3:{cacheValue}");


                await Task.Delay(300);
            }
        }

        private static async Task LocalVersion()
        {
            var cache = await KVCacheVisitorRepositoryFactory.Get().QueryByName("Cache2");

            for (var index = 0; index <= 15; index++)
            {
                //创建key1，key2,key3,三个缓存
                var cacheValue = await cache.Get<string, string>(
                     async (k) =>
                     {
                         return await Task.FromResult(Guid.NewGuid().ToString());
                     }, "Key1");

                Console.WriteLine($"Key1:{cacheValue}");

                cacheValue = await cache.Get<string, string>(
                                async (k) =>
                                {
                                    return await Task.FromResult(Guid.NewGuid().ToString());
                                }, "Key2");

                Console.WriteLine($"Key2:{cacheValue}");

                cacheValue = await cache.Get<string, string>(
                async (k) =>
                {
                    return await Task.FromResult(Guid.NewGuid().ToString());
                }, "Key3");

                Console.WriteLine($"Key3:{cacheValue}");


                await Task.Delay(100);
            }


        }

        private static async Task Combination()
        {
            var cache = await KVCacheVisitorRepositoryFactory.Get().QueryByName("Cache3");


             for (var index = 0; index <= 15; index++)
             {
                 //创建key1，key2,key3,三个缓存
                 var cacheValue = await cache.Get<string, string>(
                      async (k) =>
                      {
                          return await Task.FromResult(Guid.NewGuid().ToString());
                      }, "Key1");

                 Console.WriteLine($"Key1:{cacheValue}");

                 cacheValue = await cache.Get<string, string>(
                                 async (k) =>
                                 {
                                     return await Task.FromResult(Guid.NewGuid().ToString());
                                 }, "Key2");

                 Console.WriteLine($"Key2:{cacheValue}");

                 cacheValue = await cache.Get<string, string>(
                 async (k) =>
                 {
                     return await Task.FromResult(Guid.NewGuid().ToString());
                 }, "Key3");

                 Console.WriteLine($"Key3:{cacheValue}");


                 await Task.Delay(100);
             }

     
        }

    }
}
