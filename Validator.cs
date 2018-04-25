using System.Collections.Generic;

namespace EmailForwarder_2
{
    public class Validator
    {
        private HashSet<string> _emailsMessageIds;
        public Validator()
        {
            var repository = new EmailMessageRepositrory();
            _emailsMessageIds = new HashSet<string>(repository.GetEmailsMessageIds());
        }

        public bool IsEmailNotForwarded(string messageUid)
        {
            return !_emailsMessageIds.Contains(messageUid);
        }
    }
}
