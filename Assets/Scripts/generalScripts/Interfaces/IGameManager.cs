namespace generalScripts.Interfaces
{
    public interface IGameplayManager
    {
        void AddScore(int score);
        void OnEnemyDestroyed(int killCount);
        void UpdateHealth(float newHealth);
        void OnPlayerDied();
    }
}