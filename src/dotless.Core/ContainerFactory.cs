/* Copyright 2009 dotless project, http://www.dotlesscss.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License. */

using System.Web;

namespace dotless.Core
{
    using Abstractions;
    using configuration;
    using Microsoft.Practices.ServiceLocation;
    using Pandora;

    public class ContainerFactory
    {
        private static PandoraContainer _container;
        private static readonly object LockObject = new object();
        public IServiceLocator GetContainer()
        {
            if (_container == null)
            {
                lock (LockObject)
                {
                    if (_container == null)
                    {
                        DotlessConfiguration configuration = new WebConfigConfigurationLoader().GetConfiguration();
                        _container = CreateContainer(configuration);
                    }
                }
            }

            return new CommonServiceLocatorAdapter(_container);
        }

        private PandoraContainer CreateContainer(DotlessConfiguration configuration)
        {
            var container = new PandoraContainer();

            container.Register(p =>
                                   {
                                       p.Service<ICache>()
                                           .Implementor<CssCache>();
                                       p.Service<IPathProvider>()
                                           .Implementor<PathProvider>();
                                       p.Service<IRequest>()
                                           .Implementor<Request>();
                                       p.Service<IResponse>()
                                           .Implementor<CssResponse>();
                                   });
            container.Register(p =>
                                   {
                                       if (configuration.CacheEnabled & HttpContext.Current!=null)
                                       {
                                           p.Service<ILessEngine>()
                                               .Implementor<AspCacheDecorator>();
                                       }
                                       if (configuration.MinifyOutput)
                                       {
                                           p.Service<ILessEngine>()
                                               .Implementor<MinifierDecorator>();
                                       }
                                       p.Service<ILessEngine>()
                                           .Implementor<ExtensibleEngine>();
                                   });
            return container;
        }
    }
}