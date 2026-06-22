using Diffusion.Common.Query;
using Diffusion.Database;
using Xunit;

namespace Diffusion.Tests;

public class ComfyUIQueryBuilderTests
{
    [Fact]
    public void Parse_WithNoPrompt_ReturnsBaseQuery()
    {
        var options = new QueryOptions();

        var (query, bindings) = ComfyUIQueryBuilder.Parse(null, options);

        Assert.Contains("SELECT cmfyn.ImageId AS Id FROM Node cmfyn", query);
        Assert.Contains("WHERE", query);
        Assert.Empty(bindings);
    }

    [Fact]
    public void Parse_WithPrompt_ParameterizesValue()
    {
        var options = new QueryOptions();

        var (query, bindings) = ComfyUIQueryBuilder.Parse("landscape", options);

        Assert.Contains("(cmfyp.Value LIKE ?)", query);
        var bindingList = bindings.ToList();
        Assert.Single(bindingList);
        Assert.Equal("%landscape%", bindingList[0]);
    }

    [Fact]
    public void Parse_WithSearchProperties_ParameterizesPropertyName()
    {
        var options = new QueryOptions
        {
            SearchNodes = true,
            ComfyQueryOptions = new ComfyQueryOptions
            {
                SearchProperties = new[] { "ckpt_name" }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Parse(null, options);

        Assert.Contains("(cmfyp.Name = ?)", query);
        Assert.DoesNotContain("ckpt_name", query);
        var bindingList = bindings.ToList();
        Assert.Single(bindingList);
        Assert.Equal("ckpt_name", bindingList[0]);
    }

    [Fact]
    public void Parse_WithMaliciousPropertyName_DoesNotInjectSQL()
    {
        var malicious = "x' OR '1'='1";
        var options = new QueryOptions
        {
            SearchNodes = true,
            ComfyQueryOptions = new ComfyQueryOptions
            {
                SearchProperties = new[] { malicious }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Parse(null, options);

        Assert.DoesNotContain("OR '1'='1", query);
        Assert.Contains("(cmfyp.Name = ?)", query);
        var bindingList = bindings.ToList();
        Assert.Equal(malicious, bindingList[0]);
    }

    [Fact]
    public void Parse_WithMultipleSearchProperties_GeneratesOrClause()
    {
        var options = new QueryOptions
        {
            SearchNodes = true,
            ComfyQueryOptions = new ComfyQueryOptions
            {
                SearchProperties = new[] { "ckpt_name", "lora_name" }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Parse(null, options);

        Assert.Contains("(cmfyp.Name = ?) OR (cmfyp.Name = ?)", query);
        var bindingList = bindings.ToList();
        Assert.Equal(2, bindingList.Count);
        Assert.Equal("ckpt_name", bindingList[0]);
        Assert.Equal("lora_name", bindingList[1]);
    }

    [Fact]
    public void Filter_WithNodeFilter_ParameterizesPropertyName()
    {
        var filter = new Filter
        {
            NodeFilters = new List<NodeFilter>
            {
                new NodeFilter
                {
                    IsActive = true,
                    Property = "ckpt_name",
                    Value = "model1",
                    Comparison = NodeComparison.Equals,
                    Operation = NodeOperation.INTERSECT
                }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Filter(filter);

        Assert.Contains("cmfyp.Name = ?", query);
        Assert.DoesNotContain("'ckpt_name'", query);
        var bindingList = bindings.ToList();
        Assert.Contains("ckpt_name", bindingList);
    }

    [Fact]
    public void Filter_WithWildcardProperty_UsesLikeOperator()
    {
        var filter = new Filter
        {
            NodeFilters = new List<NodeFilter>
            {
                new NodeFilter
                {
                    IsActive = true,
                    Property = "ckpt*",
                    Value = "model1",
                    Comparison = NodeComparison.Equals,
                    Operation = NodeOperation.INTERSECT
                }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Filter(filter);

        Assert.Contains("cmfyp.Name LIKE ?", query);
        var bindingList = bindings.ToList();
        Assert.Equal("ckpt%", bindingList[0]);
    }

    [Fact]
    public void Filter_WithMaliciousPropertyName_DoesNotInjectSQL()
    {
        var malicious = "x' OR '1'='1";
        var filter = new Filter
        {
            NodeFilters = new List<NodeFilter>
            {
                new NodeFilter
                {
                    IsActive = true,
                    Property = malicious,
                    Value = "val",
                    Comparison = NodeComparison.Equals,
                    Operation = NodeOperation.INTERSECT
                }
            }
        };

        var (query, bindings) = ComfyUIQueryBuilder.Filter(filter);

        Assert.DoesNotContain("OR '1'='1", query);
        Assert.Contains("cmfyp.Name = ?", query);
    }

    [Fact]
    public void Filter_WithNoActiveNodeFilters_ReturnsEmptyQuery()
    {
        var filter = new Filter
        {
            NodeFilters = new List<NodeFilter>()
        };

        var (query, bindings) = ComfyUIQueryBuilder.Filter(filter);

        Assert.Equal("", query);
        Assert.Empty(bindings);
    }
}
