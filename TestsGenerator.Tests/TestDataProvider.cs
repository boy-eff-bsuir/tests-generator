using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Tests
{
    public static class TestDataProvider
    {
        public const string DefaultFileNamespace = "Lepesh.Lepesh";
        public const string DefaultFileClass = "LepeshClass";
        public const string DefaultFileMethod = "LepeshMethod";
        public const string ConstructorParamType = "ILepeshParam";
        public const string ConstructorParamName = "lepeshParam";
        public const string DefaultFile = @"namespace Lepesh.Lepesh
        {
            class LepeshClass
            {
                public void LepeshMethod()
                {

                }
            }
        }";
        public const string FileWithOverloadedMethod = @"namespace Lepesh.Lepesh
        {
            class LepeshClass
            {                
                public void LepeshMethod()
                {

                }

                public void LepeshMethod(int overloaded)
                {

                }
            }
        }";
        public const string FileWithEmptyNamespace = @"namespace Lepesh.Lepesh
        {
            
        }";
        public const string FileWithEmptyClass = @"namespace Lepesh.Lepesh
        {
            class LepeshClass
            {

            }
        }";
        public const string FileWithConstructor = $@"namespace Lepesh.Lepesh
        {{
            class LepeshClass
            {{
                public LepeshClass({ConstructorParamType} {ConstructorParamName})
                {{

                }}
                
                public void LepeshMethod()
                {{

                }}
            }}
        }}";
        public const string FileWithTwoConstructors = $@"namespace Lepesh.Lepesh
        {{
            class LepeshClass
            {{
                public LepeshClass({ConstructorParamType} {ConstructorParamName})
                {{

                }}

                public LepeshClass({ConstructorParamType} {ConstructorParamName}, {ConstructorParamType + "2"} {ConstructorParamName + "2"})
                {{

                }}
                
                public void LepeshMethod()
                {{

                }}
            }}
        }}";
        public const string FileWithConstructorWithNonInterfaceParam = $@"namespace Lepesh.Lepesh
        {{
            class LepeshClass
            {{
                public LepeshClass({ConstructorParamType} {ConstructorParamName}, {"Not" + ConstructorParamType} {ConstructorParamName})
                {{

                }}
                
                public void LepeshMethod()
                {{

                }}
            }}
        }}";
        public const string WrongFile = "LepeshLepeshLepeshLepesh";
    }
}
