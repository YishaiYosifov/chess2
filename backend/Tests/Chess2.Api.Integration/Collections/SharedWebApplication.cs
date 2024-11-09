using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Collections;

[CollectionDefinition(nameof(SharedWebApplication))]
public class SharedWebApplication : ICollectionFixture<Chess2WebApplicationFactory>;
