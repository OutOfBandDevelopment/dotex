using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ComponentModel.Search;
using OoBDev.System.Linq.Expressions;
using OoBDev.TestUtilities;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OoBDev.Examples.Tests.Search;

/// <summary>
/// Shows how the search annotations in <c>OoBDev.System.ComponentModel.Search</c> shape what a model exposes
/// to the filter, search and sort syntax. The same annotations drive the OpenAPI schema for search queries.
/// </summary>
[TestClass]
public class SearchAnnotationsExampleTests
{
    /// <summary>A customer record with one annotation per search concept.</summary>
    public class CustomerModel
    {
        [Key]
        [NotSortable]
        public int Id { get; set; }

        [Searchable]
        public string Name { get; set; } = default!;

        [NotFilterable]
        [NotSearchable]
        public string SocialSecurityNumber { get; set; } = default!;

        public string City { get; set; } = default!;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetFilterablePropertyNames_NotFilterable_IsHidden()
    {
        // Stage / Test
        var names = new ExpressionTreeBuilder<CustomerModel>().GetFilterablePropertyNames();

        // Assert
        CollectionAssert.DoesNotContain(names.ToList(), nameof(CustomerModel.SocialSecurityNumber));
        CollectionAssert.Contains(names.ToList(), nameof(CustomerModel.Name));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetSortablePropertyNames_NotSortable_IsHidden()
    {
        // Test
        var names = new ExpressionTreeBuilder<CustomerModel>().GetSortablePropertyNames().ToList();

        // Assert
        CollectionAssert.DoesNotContain(names, nameof(CustomerModel.Id));
        CollectionAssert.Contains(names, nameof(CustomerModel.City));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetSearchablePropertyNames_Searchable_LimitsFreeTextSearchToMarkedProperties()
    {
        // Test
        var names = new ExpressionTreeBuilder<CustomerModel>().GetSearchablePropertyNames().ToList();

        // Assert
        CollectionAssert.AreEqual(new[] { nameof(CustomerModel.Name) }, names);
    }
}
