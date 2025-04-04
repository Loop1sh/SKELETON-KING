﻿namespace ZORGATH;

public record AccountInfo(string Salt, string PasswordSalt, string HashedPassword);

/// <summary>
/// client_requester.php?f=preAuth
/// Performs the first half of the SRP key exchange. Stores the intermediate results in a ConcurrentDictionary for the
/// second half of the SRP to validate the exchange.
/// </summary>
public class PreAuthHandler : IClientRequestHandler
{
    private readonly ConcurrentDictionary<string, SrpAuthSessionData> _srpAuthSessions;

    public PreAuthHandler(ConcurrentDictionary<string, SrpAuthSessionData> srpAuthSessions)
    {
        _srpAuthSessions = srpAuthSessions;
    }

    public async Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData)
    {
        using BountyContext bountyContext = controllerContext.HttpContext.RequestServices.GetRequiredService<BountyContext>();
        string login = formData["login"];

        SrpAuthSessionData? srpAuthSessionData = await bountyContext.Accounts
            .Where(account => account.Name == login)
            .Select(account => new SrpAuthSessionData(
                login,
                formData["A"],
                account.User.Salt,
                account.User.PasswordSalt,
                account.User.HashedPassword,
                new AccountDetails(
                    account.AccountId,
                    account.Name,
                    account.AccountType,
                    account.SelectedUpgradeCodes,
                    account.AutoConnectChatChannels,
                    account.IgnoredList,

                    new AccountStats(
                        /* level: */ 0,
                        /* levelExp: */ 0,
                        /* psr: */ account.PlayerSeasonStatsPublic.AprilFirstRating,
                        /* normalRankedGamesMMR: */ account.PlayerSeasonStatsRanked.AprilFirstRating,
                        /* casualModeMMR: */ account.PlayerSeasonStatsRankedCasual.AprilFirstRating,
                        /* publicGamesPlayed: */ account.PlayerSeasonStatsPublic.AprilFirstWins + account.PlayerSeasonStatsRankedCasual.AprilFirstLosses,
                        /* normalRankedGamesPlayed: */ account.PlayerSeasonStatsRanked.AprilFirstWins + account.PlayerSeasonStatsRanked.AprilFirstLosses,
                        /* casualModeGamesPlayed: */ account.PlayerSeasonStatsRankedCasual.AprilFirstWins + account.PlayerSeasonStatsRankedCasual.AprilFirstLosses,
                        /* midWarsGamesPlayed: */ account.PlayerSeasonStatsMidWars.AprilFirstWins + account.PlayerSeasonStatsMidWars.AprilFirstLosses,
                        /* allOtherGamesPlayed: */ 0,
                        /* publicGameDisconnects: */ account.PlayerSeasonStatsPublic.TimesDisconnected,
                        /* normalRankedGameDisconnects: */ account.PlayerSeasonStatsRanked.TimesDisconnected,
                        /* casualModeDisconnects: */ account.PlayerSeasonStatsRankedCasual.TimesDisconnected,
                        /* midWarsTimesDisconnected: */ account.PlayerSeasonStatsMidWars.TimesDisconnected,
                        /* allOtherGameDisconnects: */ 0),

                    // Clan information.
                    account.ClanId,
                    account.Clan!.Name,
                    account.Clan!.Tag,
                    account.ClanTier,

                    // TODO: CloudStorage.
                    /* useCloud: */ false,
                    /* cloudAutoUpload: */ false,

                    // User-specific information.
                    account.UserId,
                    account.User.Email!,
                    account.User.GoldCoins,
                    account.User.SilverCoins,
                    account.User.UnlockedUpgradeCodes
                )))
            .FirstOrDefaultAsync();
        if (srpAuthSessionData is null)
        {
            return new NotFoundObjectResult(PHP.Serialize(new AuthFailedResponse(AuthFailureReason.AccountNotFound)));
        }

        _srpAuthSessions[login] = srpAuthSessionData;

        PreAuthResponse response = new(srpAuthSessionData.Salt, srpAuthSessionData.PasswordSalt, srpAuthSessionData.ServerEphemeral.Public);
        return new OkObjectResult(PHP.Serialize(response));
    }
}
