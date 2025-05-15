using Crud.Application.Util;
using Crud.Application.Util.Attribute;

namespace IndeaSpace.Modules.Crud.UnitTest
{
    public class MapperTest
    {
        
        internal class TestDTO
        {
            public int intVal { get; set; }
            public string? stringVal { get; set; }
            public Guid GuidVal { get; set; }

            [DoNotMap]
            public int NotMapVal { get; set; }
        }

        internal class TestModel
        {
            public int intVal { get; set; }
            public string? stringVal { get; set; }
            public Guid GuidVal { get; set; }

            [DoNotMap]
            public int NotMapVal { get; set; } = default;
        }
        internal class TestNotNullModel
        {
            public int intVal { get; set; }
            public string stringVal { get; set; }
            public Guid GuidVal { get; set; }

            [DoNotMap]
            public int NotMapVal { get; set; } = default;
        }

        [Fact(DisplayName ="Test basic map: int, string, guid and one that decorate with DoNotMap attribute")]
        public void AllExpectFeatureShouldWork()
        {
            // arrange
            TestDTO dto = new TestDTO { 
                intVal = 1,
                stringVal = "test",
                GuidVal = Guid.NewGuid(),
                NotMapVal = 10
            };

            var mapper = new SimpleMapper();

            // act
            var model = mapper.Map<TestDTO,TestModel>(dto);
            
            Assert.NotNull(model);
            Assert.Equal(model.intVal, dto.intVal);
            Assert.Equal(model.stringVal, dto.stringVal);
            Assert.Equal(model.GuidVal, dto.GuidVal);
            Assert.NotEqual(model.NotMapVal, dto.NotMapVal);
        }

        [Fact(DisplayName = "Test reverse map ignores DoNotMap as well")]
        public void ReverseMappingRespectsDoNotMap()
        {
            var model = new TestModel
            {
                intVal = 100,
                stringVal = "reverse",
                GuidVal = Guid.NewGuid(),
                NotMapVal = 99
            };

            var mapper = new SimpleMapper();
            var dto = mapper.Map<TestModel, TestDTO>(model);

            Assert.Equal(model.intVal, dto.intVal);
            Assert.Equal(model.stringVal, dto.stringVal);
            Assert.Equal(model.GuidVal, dto.GuidVal);
            Assert.Equal(default, dto.NotMapVal);
        }

        [Fact(DisplayName = "Test with nullable type, is null")]
        public void MapperShouldMapToDefaultIfNull()
        {
            var model = new TestModel
            {
                intVal = 100,
                stringVal =null,
                GuidVal = Guid.NewGuid(),
                NotMapVal = 99
            };

            var mapper = new SimpleMapper();
            var dto = mapper.Map<TestModel, TestDTO>(model);

            Assert.Equal(model.intVal, dto.intVal);
            Assert.Null(model.stringVal);
            Assert.Equal(model.GuidVal, dto.GuidVal);
            Assert.Equal(default, dto.NotMapVal);
        }

        [Fact(DisplayName = "Test with nullable type, is null")]
        public void MapperShouldMapToDefaultIfNullNotNullable()
        {
            var dto = new TestDTO
            {
                intVal = 100,
                stringVal = null,
                GuidVal = Guid.NewGuid(),
                NotMapVal = 99
            };

            var mapper = new SimpleMapper();
            var model = mapper.Map<TestDTO,TestNotNullModel>(dto);

            Assert.Equal(model.intVal, dto.intVal);
            Assert.Equal(model.stringVal, default);
            Assert.Equal(model.GuidVal, dto.GuidVal);
            Assert.Equal(model.NotMapVal, default);
        }
    }
}