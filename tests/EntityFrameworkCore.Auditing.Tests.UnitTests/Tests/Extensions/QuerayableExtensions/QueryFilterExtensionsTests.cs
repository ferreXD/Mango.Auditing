namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using FluentAssertions;
    using Setup;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class QueryFilterExtensionsTests
    {

        [Fact]
        public void WithoutSoftDeleteFilter_Should_ExcludeDeletedEntities()
        {
            // Arrange
            var existing = new SampleEntity() { Id = Guid.NewGuid(), IsDeleted = false };
            var deleted = new SampleEntity { Id = Guid.NewGuid(), IsDeleted = true };

            var queryable = new[] { existing, deleted }.AsQueryable();

            // Act
            var filtered = queryable.WithoutSoftDeleteFilter().ToList();

            // Assert
            filtered.Should().BeEquivalentTo(new[] { existing });
        }

        [Fact]
        public void WithoutSoftDeleteFilter_WithAllDeleted_ShouldReturnEmpty()
        {
            // Arrange
            var deleted1 = new SampleEntity { Id = Guid.NewGuid(), IsDeleted = true };
            var deleted2 = new SampleEntity { Id = Guid.NewGuid(), IsDeleted = true };

            var queryable = new[] { deleted1, deleted2 }.AsQueryable();

            // Act
            var filtered = queryable.WithoutSoftDeleteFilter().ToList();

            // Assert
            filtered.Should().BeEmpty();
        }

        [Fact]
        public void WithoutSoftDeleteFilter_WithNoneDeleted_ShouldReturnAll()
        {
            // Arrange
            var e1 = new SampleEntity { Id = Guid.NewGuid(), IsDeleted = false };
            var e2 = new SampleEntity { Id = Guid.NewGuid(), IsDeleted = false };

            var queryable = new[] { e1, e2 }.AsQueryable();

            // Act
            var filtered = queryable.WithoutSoftDeleteFilter().ToList();

            // Assert
            filtered.Should().BeEquivalentTo(new[] { e1, e2 });
        }

        [Fact]
        public void WithoutSoftDeleteFilter_ProducesCorrectExpressionTree()
        {
            // Arrange
            var queryable = new List<SampleEntity>().AsQueryable();

            // Act
            var filteredQueryable = queryable.WithoutSoftDeleteFilter();

            // Assert: the provider expression must be a Where call with e => e.IsDeleted == false
            var methodCall = filteredQueryable.Expression as MethodCallExpression;
            methodCall.Should().NotBeNull();
            methodCall!.Method.Name.Should().Be(nameof(Queryable.Where));

            // inspect the lambda
            var unary = methodCall.Arguments[1] as UnaryExpression;
            unary!.NodeType.Should().Be(ExpressionType.Quote);

            var lambda = unary.Operand as LambdaExpression;
            lambda!.Parameters.Single().Type.Should().Be(typeof(SampleEntity));

            // body should be e.IsDeleted == false
            var binary = lambda.Body as BinaryExpression;
            binary!.NodeType.Should().Be(ExpressionType.Equal);

            // Left should be member access of IsDeleted
            ((MemberExpression)binary.Left).Member.Name.Should().Be(nameof(ISoftDeletableEntity.IsDeleted));
            // Right should be constant false
            ((ConstantExpression)binary.Right).Value.Should().Be(false);
        }

        [Fact]
        public void WithoutSoftDeleteFilter_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IQueryable<SampleEntity> queryable = null!;

            // Act
            Action act = () => queryable.WithoutSoftDeleteFilter().ToList();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
