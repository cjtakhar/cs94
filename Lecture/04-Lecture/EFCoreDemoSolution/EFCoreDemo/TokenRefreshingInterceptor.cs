using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Azure.Core;
using Azure.Identity;

namespace EFCoreDemo
{
    /// <summary>
    /// A database connection interceptor that refreshes the Azure SQL Database access token before opening connections.
    /// </summary>
    public class TokenRefreshingInterceptor : DbConnectionInterceptor
    {
        private readonly DefaultAzureCredential _credential;
        private readonly TokenRequestContext _tokenRequestContext;
        private readonly ILogger<TokenRefreshingInterceptor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRefreshingInterceptor"/> class.
        /// </summary>
        /// <param name="credential">The Azure credential used to acquire tokens.</param>
        /// <param name="logger">The logger instance for logging connection details.</param>
        public TokenRefreshingInterceptor(DefaultAzureCredential credential,
                                          ILogger<TokenRefreshingInterceptor> logger)
        {
            _credential = credential;
            _logger = logger;
            // Resource string required for Azure SQL Database
            _tokenRequestContext = new TokenRequestContext(new[] { "https://database.windows.net//.default" });

            _logger.LogInformation("TokenRefreshingInterceptor initialized.");
        }

        /// <summary>
        /// Asynchronously intercepts the opening of a database connection to inject a fresh access token.
        /// </summary>
        /// <param name="connection">The database connection being opened.</param>
        /// <param name="eventData">The event data associated with the connection opening.</param>
        /// <param name="result">The current interception result.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>
        /// An <see cref="ValueTask{InterceptionResult}"/> representing the asynchronous operation.
        /// </returns>
        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (connection is SqlConnection sqlConnection)
            {
                // Acquire a fresh token asynchronously every time a connection is opened.
                AccessToken token = await _credential.GetTokenAsync(_tokenRequestContext, cancellationToken);
                sqlConnection.AccessToken = token.Token;

                _logger.LogInformation("Async connection opening: Using token hash: [{tokenHash}] expiring on {Expiry}.", token.GetHashCode(), token.ExpiresOn);
                _logger.LogDebug("Async connection opening: Connection DataSource - {DataSource}, Database - {Database}.", sqlConnection.DataSource, sqlConnection.Database);
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }

        /// <summary>
        /// Intercepts the opening of a database connection to inject a fresh access token synchronously.
        /// </summary>
        /// <param name="connection">The database connection being opened.</param>
        /// <param name="eventData">The event data associated with the connection opening.</param>
        /// <param name="result">The current interception result.</param>
        /// <returns>An <see cref="InterceptionResult"/> representing the operation result.</returns>
        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            if (connection is SqlConnection sqlConnection)
            {
                // Acquire a fresh token synchronously.
                AccessToken token = _credential.GetToken(_tokenRequestContext, default);
                sqlConnection.AccessToken = token.Token;

                _logger.LogInformation("Sync connection opening: Using token hash: [{tokenHash}] expiring on {Expiry}.", token.GetHashCode(), token.ExpiresOn);
                _logger.LogDebug("Sync connection opening: Connection DataSource - {DataSource}, Database - {Database}.", sqlConnection.DataSource, sqlConnection.Database);
            }
            return base.ConnectionOpening(connection, eventData, result);
        }
    }
}