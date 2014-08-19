using System;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace EBE.Data
{
    /// <summary>
    /// Simple database interface.
    /// </summary>
    public class EBEContext : IDisposable
    {
        private static bool _created = false;
        private static object _lock = new object();

        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;

        /// <summary>
        /// Gets the session factory thinger.
        /// </summary>
        private static ISessionFactory SessionFactory
        {
            get
            {
                lock (_lock)
                {
                    return _sessionFactory;
                }
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="EBE.Data.EBEContext"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="EBE.Data.EBEContext"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="EBE.Data.EBEContext"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="EBE.Data.EBEContext"/> so
        /// the garbage collector can reclaim the memory that the <see cref="EBE.Data.EBEContext"/> was occupying.</remarks>
        public void Dispose()
        {
        }

        /// <summary>
        /// Opens the database session.
        /// </summary>
        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        /// <summary>
        /// Gets the encyclopedia repository.
        /// </summary>
        public IRepository<Encyclopedia> Encyclopedia
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the gen repository.
        /// </summary>
        public IRepository<Gen> Gen
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Data.EBEContext"/> class.
        /// </summary>
        public EBEContext()
        {
            Setup();

            if (Encyclopedia == null)
            {
                Encyclopedia = new Repository<Encyclopedia>();
            }

            if (Gen == null)
            {
                Gen = new Repository<Gen>();
            }
        }

        /// <summary>
        /// Save item to database.
        /// </summary>
        internal void TransactSave(object obj)
        {
            using(ISession session = OpenSession())
            using(ITransaction transaction = session.BeginTransaction())
            {
                session.Save(obj);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Update item in database.
        /// </summary>
        internal void TransactUpdate(object obj)
        {
            using(ISession session = OpenSession())
            using(ITransaction transaction = session.BeginTransaction())
            {
                session.Update(obj);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Delete item from database.
        /// </summary>
        internal void TransactDelete(object obj)
        {
            using(ISession session = OpenSession())
            using(ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(obj);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Get item from database.
        /// </summary>
        /// <returns>Item to find.</returns>
        /// <param name="key">Key to find item.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        internal T TransactGet<T>(object key)
        {
            using(ISession session = _sessionFactory.OpenSession())
            {
                return session.Get<T>(key);
            }
        }

        /// <summary>
        /// Find objects that have property with the given value.
        /// </summary>
        /// <returns>Finds items that match the given property with the given value.</returns>
        /// <param name="propertyName">Property name to look at.</param>
        /// <param name="value">Value for the property.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        internal List<T> TransactFind<T>(string propertyName, object value) where T : class
        {
            using(ISession session = _sessionFactory.OpenSession())
            {
                var results = session.CreateCriteria<T>()
                              .Add(Restrictions.Eq(propertyName, value))
                              .List<T>();
                List<T> list = new List<T>();

                foreach (var r in results)
                {
                    list.Add(r);
                }

                return list;
            }
        }

        /// <summary>
        /// Finds items that matches the given properties with the given values.
        /// </summary>
        /// <returns>List of items that match.</returns>
        /// <param name="propertyName">Property name to look at.</param>
        /// <param name="value">Value for the property.</param>
        /// <param name="propertyName2">Second property name to look at.</param>
        /// <param name="value2">Value for the second property.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        internal List<T> TransactFind2<T>(string propertyName,
                                          object value,
                                          string propertyName2,
                                          object value2) where T : class
        {
            using(ISession session = _sessionFactory.OpenSession())
            {
                var results = session.CreateCriteria<T>()
                              .Add(Restrictions.Eq(propertyName, value))
                              .Add(Restrictions.Eq(propertyName2, value2))
                              .List<T>();
                List<T> list = new List<T>();

                foreach (var r in results)
                {
                    list.Add(r);
                }

                return list;
            }
        }

        private void Setup()
        {
            if (_created == false)
            {
                lock (_lock)
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
            cfg.AddAssembly(typeof(Encyclopedia).Assembly);

            new SchemaExport(cfg);
            //new SchemaExport(cfg).Execute(false, true, false, false);
        }
    }
}

