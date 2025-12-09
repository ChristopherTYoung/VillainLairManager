using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service interface for evil scheme-related business logic
    /// </summary>
    public interface ISchemeService
    {
        /// <summary>
        /// Calculates the success likelihood of a scheme based on assigned resources
        /// </summary>
        int CalculateSuccessLikelihood(EvilScheme scheme);

        /// <summary>
        /// Updates the success likelihood for a scheme
        /// </summary>
        void UpdateSuccessLikelihood(EvilScheme scheme);

        /// <summary>
        /// Validates if a scheme is over budget
        /// </summary>
        bool IsOverBudget(EvilScheme scheme);

        /// <summary>
        /// Gets all schemes
        /// </summary>
        List<EvilScheme> GetAllSchemes();

        /// <summary>
        /// Gets a scheme by ID
        /// </summary>
        EvilScheme GetSchemeById(int schemeId);

        /// <summary>
        /// Gets all active schemes
        /// </summary>
        List<EvilScheme> GetActiveSchemes();

        /// <summary>
        /// Calculates average success likelihood for a list of schemes
        /// </summary>
        double CalculateAverageSuccess(List<EvilScheme> schemes);
    }
}
