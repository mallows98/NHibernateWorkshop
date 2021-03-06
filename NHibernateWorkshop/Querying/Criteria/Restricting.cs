using NHibernate.Criterion;
using Northwind.Builders;
using Northwind.Entities;
using NUnit.Framework;
using System.Linq;

namespace NHibernateWorkshop.Querying.Criteria
{
    [TestFixture]
    public class Restricting : AutoRollbackFixture
    {
        private Product _product1;
        private Product _product2;
        private Product _product3;

        protected override void AfterSetUp()
        {
            _product1 = new ProductBuilder().WithName("product 1").Build();
            _product2 = new ProductBuilder().WithName("product 2").Build();
            _product3 = new ProductBuilder().WithName("product 3").Build();
            Session.Save(_product1);
            Session.Save(_product2);
            Session.Save(_product3);
            Flush();
        }

        [Test]
        public void where_properties_are_equal()
        {
            _product1.UnitsInStock = 10;
            _product1.UnitsOnOrder = 10;
            _product2.UnitsInStock = 13;
            _product2.UnitsOnOrder = 14;
            _product3.UnitsInStock = 7;
            _product3.UnitsOnOrder = 7;
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.EqProperty("UnitsInStock", "UnitsOnOrder"))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.AreEqual(p.UnitsInStock, p.UnitsOnOrder));
        }

        [Test]
        public void where_one_property_is_greater_than_the_other()
        {
            _product1.UnitsInStock = 10;
            _product1.UnitsOnOrder = 10;
            _product2.UnitsInStock = 13;
            _product2.UnitsOnOrder = 14;
            _product3.UnitsInStock = 2;
            _product3.UnitsOnOrder = 7;
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.GtProperty("UnitsOnOrder", "UnitsInStock"))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.Greater(p.UnitsOnOrder, p.UnitsInStock));
        }

        [Test]
        public void where_one_property_has_a_value_between_a_given_range()
        {
            _product1.UnitsInStock = 10;
            _product2.UnitsInStock = 13;
            _product3.UnitsInStock = 9;
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Between("UnitsInStock", 5, 10))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.UnitsInStock >= 5 && p.UnitsInStock <= 10));
        }

        [Test]
        public void where_one_property_has_a_value_in_a_given_set()
        {
            _product1.UnitsInStock = 10;
            _product2.UnitsInStock = 13;
            _product3.UnitsInStock = 9;
            Flush();

            var stockLevels = new[] {7, 9, 11, 13};
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.In("UnitsInStock", stockLevels))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(stockLevels.Contains(p.UnitsInStock.Value)));
        }

        [Test]
        public void where_string_property_contains_value()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Like("Name", "roduc", MatchMode.Anywhere))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.Name.Contains("roduc")));
        }

        [Test]
        public void where_string_property_begins_with_value()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Like("Name", "pr", MatchMode.Start))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.Name.StartsWith("pr")));
        }

        [Test]
        public void where_string_property_ends_with_value()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Like("Name", "3", MatchMode.End))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.Name.EndsWith("3")));
        }

        [Test]
        public void where_string_property_matches_value_exactly()
        {
            var product4 = new ProductBuilder().WithName("blah product 2 blah").Build();
            Session.Save(product4);
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Like("Name", "product 2", MatchMode.Exact))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsFalse(products.Contains(_product3));
            Assert.IsFalse(products.Contains(product4));
            products.Each(p => Assert.AreEqual("product 2", p.Name));
        }

        [Test]
        public void where_property_is_null()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.IsNull("ReorderLevel"))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsNull(p.ReorderLevel));
        }

        [Test]
        public void where_property_is_not_null()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.IsNotNull("ReorderLevel"))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsFalse(products.Contains(_product3));
            products.Each(p => Assert.IsNotNull(p.ReorderLevel));
        }

        [Test]
        public void where_collection_is_empty()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.IsEmpty("Sources"))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.AreEqual(0, p.Sources.Count()));
        }

        [Test]
        public void where_collection_is_not_empty()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.IsNotEmpty("Sources"))
                .List<Product>();

            Assert.IsFalse(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsFalse(products.Contains(_product3));
            products.Each(p => Assert.Greater(p.Sources.Count(), 0));
        }

        [Test]
        public void two_restrictions_with_and()
        {
            _product1.AddSource(new SupplierBuilder().Build(), 10);
            _product1.UnitsInStock = 5;
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.And(
                    Restrictions.IsNotEmpty("Sources"),
                    Restrictions.IsNotNull("UnitsInStock")))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsFalse(products.Contains(_product3));
            products.Each(p =>
            {
                Assert.Greater(p.Sources.Count(), 0);
                Assert.IsNotNull(p.UnitsInStock);
            });
        }

        [Test]
        public void two_restrictions_with_or()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Or(
                    Restrictions.IsNotEmpty("Sources"),
                    Restrictions.IsNull("UnitsInStock")))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.Sources.Count() > 0 || p.UnitsInStock == null));
        }

        [Test]
        public void more_than_two_restrictions_with_and()
        {
            _product1.AddSource(new SupplierBuilder().Build(), 10);
            _product1.UnitsInStock = 5;
            _product3.ReorderLevel = null;
            Flush();

            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Conjunction()
                    .Add(Restrictions.IsNotEmpty("Sources"))
                    .Add(Restrictions.IsNotNull("UnitsInStock"))
                    .Add(Restrictions.IsNull("ReorderLevel")))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsFalse(products.Contains(_product2));
            Assert.IsFalse(products.Contains(_product3));
            products.Each(p =>
            {
                Assert.Greater(p.Sources.Count(), 0);
                Assert.IsNotNull(p.UnitsInStock);
                Assert.IsNull(p.ReorderLevel);
            });
        }

        [Test]
        public void more_than_two_restrictions_with_or()
        {
            var products = Session.CreateCriteria<Product>()
                .Add(Restrictions.Disjunction()
                    .Add(Restrictions.IsNotEmpty("Sources"))
                    .Add(Restrictions.IsNull("UnitsInStock"))
                    .Add(Restrictions.IsNotNull("ReorderLevel")))
                .List<Product>();

            Assert.IsTrue(products.Contains(_product1));
            Assert.IsTrue(products.Contains(_product2));
            Assert.IsTrue(products.Contains(_product3));
            products.Each(p => Assert.IsTrue(p.Sources.Count() > 0 || p.UnitsInStock == null || p.ReorderLevel.HasValue));
        }
    }
}