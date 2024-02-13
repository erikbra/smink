namespace NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer;

public class When_the_customer_is_too_old
{
    [Test]
    public void We_send_an_apology_to_the_customer(){}
    
    [Test]
    public void We_refer_the_customer_to_an_affiliate_shop(){}
    
    [Test]
    public void We_make_them_happy(){ Assert.Fail(); }
    
}