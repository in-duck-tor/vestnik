using FirebaseAdmin.Messaging;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public interface IInDuckTorBankMessageSender
{
       
}

internal class InDuckTorBankFirebaseMessageSender : IInDuckTorBankMessageSender
{
    private readonly InDuckTorBankFirebaseApp _firebaseApp;
    private readonly FirebaseMessaging _firebaseMessaging;

    public InDuckTorBankFirebaseMessageSender(InDuckTorBankFirebaseApp firebaseApp)
    {
        _firebaseApp = firebaseApp;
        _firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp.App);
    }
}