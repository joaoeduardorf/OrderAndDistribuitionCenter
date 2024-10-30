using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Domain.Notifications
{
    public class DomainNotificationHandler
    {
        private readonly List<DomainNotification> _notifications = new List<DomainNotification>();

        // Adiciona uma nova notificação com uma chave e mensagem de erro
        public void AddNotification(string key, string message)
        {
            _notifications.Add(new DomainNotification(key, message));
        }

        // Verifica se há notificações armazenadas
        public bool HasNotifications() => _notifications.Any();

        // Retorna todas as notificações armazenadas
        public List<DomainNotification> GetNotifications() => _notifications;
    }

    // Representa uma notificação de domínio com chave e mensagem de erro
    public class DomainNotification
    {
        public string Key { get; }
        public string Message { get; }

        public DomainNotification(string key, string message)
        {
            Key = key;
            Message = message;
        }
    }
}
