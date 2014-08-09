using System;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace EBE.Data
{
    public class EBEContext : IDisposable
    {
        private static bool _created = false;
        private static object _lock = new object();

        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;

        private static ISessionFactory SessionFactory
        {
            get
            { 
                lock(_lock)
                {
                    return _sessionFactory;
                }
            }
        }

        public void Dispose()
        {
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public IRepository<Encyclopedia> Encyclopedia
        {
            get;

            private set;
        }

        public IRepository<Gen> Gen
        {
            get;

            private set;
        }

        public EBEContext()
        {
            Setup();

            if(Encyclopedia == null)
            {
                Encyclopedia = new Repository<Encyclopedia>();
            }

            if(Gen == null)
            {
                Gen = new Repository<Gen>();
            }
        }

        internal void TransactSave(object obj)
        {
            using (ISession session = OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(obj);
                transaction.Commit();
            }
        }

        internal void TransactUpdate(object obj)
        {
            using (ISession session = OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(obj);
                transaction.Commit();
            }
        }

        internal void TransactDelete(object obj)
        {
            using (ISession session = OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(obj);
                transaction.Commit();
            }
        }

        internal T TransactGet<T>(object key)
        {
            using (ISession session = _sessionFactory.OpenSession())
            {
                return session.Get<T>(key);
            }
        }

        internal List<T> TransactFind<T>(string propertyName, object value) where T : class
        {
            using (ISession session = _sessionFactory.OpenSession())
            {
                var results = session.CreateCriteria<T>()
                                .Add(Restrictions.Eq(propertyName, value))
                                .List<T>();

                List<T> list = new List<T>();

                foreach(var r in results)
                {
                    list.Add(r);
                }

                return list;
            }
        }

        internal List<T> TransactFind2<T>(string propertyName, object value, string propertyName2, object value2) where T : class
        {
            using (ISession session = _sessionFactory.OpenSession())
            {
                var results = session.CreateCriteria<T>()
                    .Add(Restrictions.Eq(propertyName, value))
                    .Add(Restrictions.Eq(propertyName2, value2))
                        .List<T>();

                List<T> list = new List<T>();

                foreach(var r in results)
                {
                    list.Add(r);
                }

                return list;
            }
        }

        private void Setup()
        {
            if(_created == false)
            {
                lock(_lock)
                {
                    _configuration = new Configuration();
                    _configuration.Configure();
                    _configuration.AddAssembly(typeof(Encyclopedia).Assembly);
                    _sessionFactory = _configuration.BuildSessionFactory();

                    _created = true;
                }
            }
        }

        private void Generate()
        {
            // should (somehow) drop and create table
            // http://nhforge.org/wikis/howtonh/your-first-nhibernate-based-application.aspx
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof (Encyclopedia).Assembly);

            new SchemaExport(cfg);
            //new SchemaExport(cfg).Execute(false, true, false, false);
        }
    }
}

