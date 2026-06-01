using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.WebApi.API
{
    public static class AIHyperparametersApi
    {
        public static IEndpointRouteBuilder MapAIHyperparametersApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("ai/hyperparameters").WithTags("AIHyperparameters");

            api.MapGet("", GetHyperparameters)
                .WithOpenApi(op => { op.Summary = "Get current global AI hyperparameters"; return op; });

            api.MapPost("", UpdateHyperparameters)
                .WithOpenApi(op => { op.Summary = "Manually update global AI hyperparameters"; return op; });

            api.MapPost("reset", ResetHyperparameters)
                .WithOpenApi(op => { op.Summary = "Reset global AI hyperparameters to default baseline"; return op; });

            api.MapGet("history", GetHyperparameterHistory)
                .WithOpenApi(op => { op.Summary = "Get history of AI hyperparameters for monitoring charts"; return op; });

            return app;
        }

        private static async Task<IResult> GetHyperparameters(
            [FromServices] IQueryBuilder<AIHyperparameter> hyperparameterBuilder,
            [FromServices] IQueryExecutor queryExecutor,
            CancellationToken cancellationToken)
        {
            var query = hyperparameterBuilder.QueryAsNoTracking.Where(h => h.Id == 1);
            var hyperparams = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (hyperparams == null)
            {
                return TypedResults.Ok(new AIHyperparameter { Id = 1, K = 1.5, IdleWeight = 0.1, LearningRate = 0.01 });
            }
            return TypedResults.Ok(hyperparams);
        }

        public record UpdateAIHyperparametersRequest(double K, double IdleWeight, double LearningRate);

        private static async Task<IResult> UpdateHyperparameters(
            [FromBody] UpdateAIHyperparametersRequest request,
            [FromServices] IRepository<AIHyperparameter> hyperparameterRepository,
            [FromServices] IQueryBuilder<AIHyperparameter> hyperparameterBuilder,
            [FromServices] IQueryExecutor queryExecutor,
            CancellationToken cancellationToken)
        {
            var query = hyperparameterBuilder.Query.Where(h => h.Id == 1);
            var hyperparams = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (hyperparams == null)
            {
                hyperparams = new AIHyperparameter { Id = 1 };
                hyperparams.K = Math.Max(0.0, request.K);
                hyperparams.IdleWeight = Math.Max(0.0, request.IdleWeight);
                hyperparams.LearningRate = Math.Max(0.0, request.LearningRate);
                await hyperparameterRepository.Add(hyperparams, cancellationToken);
            }
            else
            {
                hyperparams.K = Math.Max(0.0, request.K);
                hyperparams.IdleWeight = Math.Max(0.0, request.IdleWeight);
                hyperparams.LearningRate = Math.Max(0.0, request.LearningRate);
                await hyperparameterRepository.Update(hyperparams, cancellationToken);
            }

            await hyperparameterRepository.SaveChangesAsync(cancellationToken);
            return TypedResults.Ok(hyperparams);
        }

        private static async Task<IResult> ResetHyperparameters(
            [FromServices] IRepository<AIHyperparameter> hyperparameterRepository,
            [FromServices] IQueryBuilder<AIHyperparameter> hyperparameterBuilder,
            [FromServices] IQueryExecutor queryExecutor,
            CancellationToken cancellationToken)
        {
            var query = hyperparameterBuilder.Query.Where(h => h.Id == 1);
            var hyperparams = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (hyperparams == null)
            {
                hyperparams = new AIHyperparameter { Id = 1, K = 1.5, IdleWeight = 0.1, LearningRate = 0.01 };
                await hyperparameterRepository.Add(hyperparams, cancellationToken);
            }
            else
            {
                hyperparams.K = 1.5;
                hyperparams.IdleWeight = 0.1;
                hyperparams.LearningRate = 0.01;
                await hyperparameterRepository.Update(hyperparams, cancellationToken);
            }

            await hyperparameterRepository.SaveChangesAsync(cancellationToken);
            return TypedResults.Ok(hyperparams);
        }

        private static async Task<IResult> GetHyperparameterHistory(
            [FromServices] IQueryBuilder<AIHyperparameterHistory> historyBuilder,
            [FromServices] IQueryExecutor queryExecutor,
            CancellationToken cancellationToken)
        {
            var query = historyBuilder.QueryAsNoTracking.OrderBy(h => h.Timestamp);
            var history = await queryExecutor.ToListAsync(query, cancellationToken);
            return TypedResults.Ok(history);
        }
    }
}
