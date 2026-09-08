using FEAR.Domain.Configuration;
using FEAR.Domain.Telemetry;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Runtime.Telemetry
{
    public class DefaultFEARTelemetryDestinationProvider : IFEARTelemetryDestinationProvider, IDisposable
    {
        private Dictionary<string, IFEARTelemetryDestination> _destinations = new Dictionary<string, IFEARTelemetryDestination>();
        public Func<IFEARTelemetrySignal, string> FormatSignal { get; set; } = (signal) => $"{signal.Timestamp.ToString("o")}-{signal.SignalType}-{signal.SignalSource}::{signal.SignalData}";

        private Lazy<Dictionary<string, Type>> LazyNamedDestinationTypes;
        private Dictionary<string, Type> NamedDestinationTypes => LazyNamedDestinationTypes.Value;

        private IConfiguration _configuration;
        public string DefaultWorkingDirectory { get; }

        public DefaultFEARTelemetryDestinationProvider(IConfiguration configuration, string defaultWorkingDirectory)
        {
            _configuration = configuration;
            DefaultWorkingDirectory = defaultWorkingDirectory;
            LazyNamedDestinationTypes = new Lazy<Dictionary<string, Type>>(() =>
            {
                // check for FEARTelemetryDestinationAttribute to get the name
                var destinationTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => typeof(IFEARTelemetryDestination).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    .Where(type => type.GetCustomAttributes(typeof(FEARTelemetryDestinationAttribute), false).Length > 0);

                return destinationTypes.ToDictionary(
                    type => ((FEARTelemetryDestinationAttribute)type.GetCustomAttributes(typeof(FEARTelemetryDestinationAttribute), false).First()).DestinationName,
                    type => type
                );
            });
        }

        public IDictionary<string, IFEARTelemetryDestination> GetDestinations()
        {
            return _destinations;
        }

        public Dictionary<string, object> GetDefaultSettingsForDestinationName(string destinationName)
        {
            var defaultSettings = new Dictionary<string, object>();
            var section = _configuration.GetSection($"Telemetry:Destinations:{destinationName}:DefaultSettings");
            if (section != null)
            {
                string configuration = section.Value;
                if (!string.IsNullOrEmpty(configuration))
                {
                    DelimitedParameterParser.ParseSectionIntoDictionary(configuration, defaultSettings);
                }
            }

            return defaultSettings;
        }

        public IFEARTelemetryDestination? RegisterDestination(string destinationTypeName, string destinationIdentifier, Dictionary<string, object> settings)
        {
            return RegisterDestination(destinationTypeName, destinationIdentifier, settings, new GenericFEARTelementryHandlerPredicates()
            {
                SignalPredicates = new List<Predicate<IFEARTelemetrySignal>>()
                {
                    (signal) => signal.SignalSource == destinationIdentifier
                }
            });
        }

        public IFEARTelemetryDestination? RegisterDestination(string destinationTypeName, string destinationIdentifier, Dictionary<string, object> settings, IFEARTelementryHandlerPredicates predicates)
        {
            if(_destinations.ContainsKey(destinationIdentifier))
            {
                return _destinations[destinationIdentifier];
            }

            if (NamedDestinationTypes.ContainsKey(destinationTypeName))
            {
                var destinationType = NamedDestinationTypes[destinationTypeName];
                var destinationConfiguration = new FEARTelemetryDestinationConfiguration()
                {
                    Predicates = predicates,
                    Settings = settings,
                    DestinationIdentifier = destinationIdentifier
                };
                destinationConfiguration.MergeDefaultSettings(GetDefaultSettingsForDestinationName(destinationTypeName));
                IFEARTelemetryDestination? destinationInstance = (IFEARTelemetryDestination)Activator.CreateInstance(destinationType, this, destinationConfiguration);
                
                if (destinationInstance == null)
                {
                    throw new InvalidOperationException($"Could not create instance of telemetry destination type '{destinationTypeName}'.");
                }

                _destinations.Add(destinationIdentifier, destinationInstance);
                return destinationInstance;
            }
            else
            {
                throw new ArgumentException($"Telemetry destination '{destinationTypeName}' not found.");
            }
        }

        public void Dispose()
        {
            // Dispose logic here if needed
            foreach (var destination in _destinations.Values)
            {
                destination.Dispose();
            }
        }
    }
}
