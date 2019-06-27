import * as React from 'react';

interface FetchDataExampleState {
    data: any;
    loading: boolean;
}

export class FetchData extends React.Component<{}, FetchDataExampleState> {
    constructor() {
        super();
        this.state = { data: {}, loading: true };


        let qsData:any = {
            grant_type: 'client_credentials',
            client_id: 'diegomary',
            scope: 'DM_secure_API',
            client_secret: 'dmpassword'
        };

        let esc = encodeURIComponent;
        let body = `${Object.keys(qsData).map(k => `${esc(k)}=${esc(qsData[k])}`).join('&')}`;

        fetch('http://localhost:5000/connect/token', {
            method: 'post', 
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body
        })
            .then(response => response.json() as Promise<any>)
            .then(data => {
                this.setState({ data: data.access_token, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading ? <p><em>Loading...</em></p> : this.state.data;
        return <div>
            <h1>Identity Server Token</h1>
            <p></p>
            { contents }
        </div>;
    }
   
}


