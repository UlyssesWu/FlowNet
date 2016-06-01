from mininet.topo import Topo

class TestTopo(Topo):
    "Simple Test Topo"

    def __init__(self):
        "Custom Topo Test"

        # Initialize topology
        Topo.__init__( self )

        # Add hosts and switches
        h1 = self.addHost( 'h1' )
        #hl2 = self.addHost( 'hl2' )
        #hl3 = self.addHost( 'hl3' )

        h2 = self.addHost( 'h2' )
        #hr2 = self.addHost( 'hr2' )
        #hr3 = self.addHost( 'hr3' )

        #Hl = [hl1,hl2,hl3]
        #Hr = [hr1,hr2,hr3]
        Hl = [h1]
        Hr = [h2]

        s1 = self.addSwitch( 's1' )
        s2 = self.addSwitch( 's2' )
        s3 = self.addSwitch( 's3' )

        # Add links
        #self.addLink(s1,s2,bw=10,delay='5ms',loss=10,max_queue_size=1000,use_htb=True)
        #self.addLink( s1,s2, delay='5ms',loss=1)
	self.addLink( s1,s2)
        self.addLink( s1,s3)
        self.addLink( s3,s2)

        for h in Hl:
            self.addLink(h,s1)
        for h in Hr:
            self.addLink(h,s2)

topos = {'mytopo':(lambda : TestTopo())}
